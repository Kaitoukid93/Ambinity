using System.Buffers;
using System.Diagnostics;
using System.IO.Ports;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using AmbinityCore.Enums;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.LED;
using Serilog;

namespace AmbinityCore.DataStream;

internal sealed class SerialStream : IDisposable, IDataStream
{
    public event Action<IController> ControllerDisconnected;
    private byte[] testBuffer = new byte[1216];

    public SerialStream(IController controller)
    {
        Controller = controller as SerialController;
    }

    public void Init()
    {
        ID = Controller.SerialNumber;
        Port = Controller.SerialPort;
        Controller.WorkingStateEnum = ControllerWorkingStateEnum.Normal;
        _hasPWMCOntroller = Controller.FanController != null;
        Start();
    }

    //Dependency Injection//
    public IController Controller { get; set; }
    public string ID { get; set; }

    private void DeviceStateChanged()
    {
        if (Controller.WorkingStateEnum == ControllerWorkingStateEnum.Normal)
        {
            _dimMode = DimMode.Up;
            _dimFactor = 0.00;
        }
        else if (Controller.WorkingStateEnum == ControllerWorkingStateEnum.Off)
        {
            _dimMode = DimMode.Down;
            _dimFactor = 1.00;
        }
    }


    /// <summary>
    /// private properties
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    private Thread _workerThread;
    private bool _hasPWMCOntroller;
    private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };

    public string Port { get; private set; }

    public bool IsValid() => SerialPort.GetPortNames().Contains(Controller.SerialPort);

    private double _dimFactor;

    private enum DimMode
    {
        Up,
        Down
    };

    private DimMode _dimMode;

    private void DimLED()
    {
        if (_dimMode == DimMode.Down)
        {
            if (_dimFactor >= 0.01)
                _dimFactor -= 0.01;
            if (_dimFactor < 0.01)
                _dimFactor = 0;
            // if (_dimFactor < 0.1)
            //  _dimMode = DimMode.Up;
        }
        else if (_dimMode == DimMode.Up)
        {
            if (_dimFactor <= 0.99)
                _dimFactor += 0.01;
            //_dimMode = DimMode.Down;
        }
    }

    public void Start()
    {
        Log.Information("Start called for SerialStream");
        if (_workerThread == null || !_workerThread.IsAlive)
            _workerThread = new Thread(DoWork)
            {
                Name = "Serial sending",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
        _cancellationTokenSource = new CancellationTokenSource();
        _workerThread.Start(_cancellationTokenSource.Token);
        Controller.EnableTransfer();
    }

    public void Stop()
    {
        Controller.DisableTransfer();
        Log.Information("Stop called for Serial Stream");
        if (_workerThread == null) return;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
    }

    public bool IsRunning => _workerThread != null && _workerThread.IsAlive;


    private (byte[] Buffer, int OutputLength) GetOutputStream(int id)
    {
        byte[] outputStream;
        var ambinityDevice = Controller.LedController.Outputs[id].Device;
        int counter = _messagePreamble.Length;
        const int colorsPerLed = 3;
        const int hilocheckLenght = 3;
        const int extraHeader = 3;
        int ledCount = ambinityDevice.Leds.Count;
        int bufferLength = _messagePreamble.Length + hilocheckLenght + extraHeader + (ledCount * colorsPerLed);

        outputStream = ArrayPool<byte>.Shared.Rent(bufferLength);
        Buffer.BlockCopy(_messagePreamble, 0, outputStream, 0, _messagePreamble.Length);

        byte lo = (byte)((ledCount == 0 ? 1 : ledCount) & 0xff);
        byte hi = (byte)(((ledCount == 0 ? 1 : ledCount) >> 8) & 0xff);
        byte chk = (byte)(hi ^ lo ^ 0x55);


        outputStream[counter++] = hi;
        outputStream[counter++] = lo;
        outputStream[counter++] = chk;
        outputStream[counter++] = (byte)id;
        outputStream[counter++] = 200;
        outputStream[counter++] = 0;

        double brightnessCap = Controller.LedController.MaxBrightness / 100d;
        var allBlack = true;
        int aliveSpotCounter = 0;
        var rgbOrder = ambinityDevice.RGBOrder;
        DimLED();

        lock (ambinityDevice.Lock)
        {
            if (ambinityDevice.Leds.Count == 0) //this could be PID has removed all items add 1 dummy
            {
                outputStream[counter++] = 0; // blue
                outputStream[counter++] = 0; // green
                outputStream[counter++] = 0; // red
            }
            else
            {
                foreach (AmbinityLED led in ambinityDevice.Leds)
                {
                    ApplyColorWhitebalance(led.LED.Red, led.LED.Green, led.LED.Blue,
                        ambinityDevice.RedScale, ambinityDevice.GreenScale,
                        ambinityDevice.BlueScale,
                        out byte FinalR, out byte FinalG, out byte FinalB);
                    ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB, out byte r, out byte g, out byte b);
                    //get data
                    outputStream[counter + led.Index * 3 + 0] = led.LED.Green;

                    outputStream[counter + led.Index * 3 + 1] = led.LED.Blue;
                    // green
                    outputStream[counter + led.Index * 3 + 2] = led.LED.Red;
                    // red
                    aliveSpotCounter++;


                    allBlack = allBlack && led.LED.Red == 0 && led.LED.Green == 0 && led.LED.Blue == 0;
                }
            }
        }

        for (int i = counter + aliveSpotCounter * 3; i < bufferLength; i++)
        {
            outputStream[i] = 0;
        }

        return (outputStream, bufferLength);
    }

    private void ApplyColorWhitebalance(byte r, byte g, byte b, int whiteBalanceRed, int whiteBalanceGreen,
        int whiteBalanceBlue, out byte finalR, out byte finalG, out byte finalB)
    {
        finalR = (byte)(r * whiteBalanceRed / 100);
        finalG = (byte)(g * whiteBalanceGreen / 100);
        finalB = (byte)(b * whiteBalanceBlue / 100);
    }

    private void ReOrderSpotColor(RGBLEDOrderEnum order, byte rawR, byte rawG, byte rawB, out byte r, out byte g,
        out byte b)
    {
        r = rawR;
        g = rawG;
        b = rawB;
        switch (order)
        {
            case RGBLEDOrderEnum.RGB:
                r = rawR;
                g = rawG;
                b = rawB;
                break;
            case RGBLEDOrderEnum.RBG:
                r = rawR;
                g = rawB;
                b = rawG;
                break;
            case RGBLEDOrderEnum.BGR:
                r = rawB;
                g = rawG;
                b = rawR;
                break;
            case RGBLEDOrderEnum.BRG:
                r = rawB;
                g = rawR;
                b = rawG;
                break;
            case RGBLEDOrderEnum.GRB:
                r = rawG;
                g = rawR;
                b = rawB;
                break;
            case RGBLEDOrderEnum.GBR:
                r = rawG;
                g = rawB;
                b = rawR;
                break;
        }
    }

    private void DoWork(object tokenObject)
    {
        var cancellationToken = (CancellationToken)tokenObject;

        if (String.IsNullOrEmpty(Controller.SerialPort))
        {
            Log.Warning("Cannot start the serial sending because the comport is not selected.");
            return;
        }

        int baudRate = 2000000;
        // if ((Controller as SerialController).CustomBaudrateEnabled)
        // {
        //     baudRate = (Controller as SerialController).Baudrate;
        // }
        // else
        // {
        //     if (Controller.HardwareType == HardwareTypeEnum.AmbinoFanHub ||
        //         Controller.HardwareType == HardwareTypeEnum.AmbinoHUBV3)
        //         baudRate = 2500000;
        // }

        var _serialPort = new SerialPort(Controller.SerialPort, baudRate);
        try
        {
            _serialPort.Open();
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Error(_serialPort.PortName + " is in use");
            ControllerDisconnected?.Invoke(this.Controller);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            ControllerDisconnected?.Invoke(this.Controller);
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //send frame data
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var outputCount = Controller.LedController.Outputs.Count;
                var singleOutputBufferLength = 255 * 3 + 9;
                var buffer = ArrayPool<byte>.Shared.Rent(singleOutputBufferLength * outputCount);
                int bufferLength = 0;
                for (int i = 0; i < Controller.LedController.Outputs.Count; i++)
                {
                    if (!Controller.LedController.Outputs[i].IsEnabled)
                        continue;
                    var (outputBuffer, streamLength) = GetOutputStream(i);
                    Buffer.BlockCopy(outputBuffer, 0, buffer, bufferLength, streamLength);
                    bufferLength += streamLength;
                }

                _serialPort.Write(buffer, 0, bufferLength);
                ArrayPool<byte>.Shared.Return(buffer);
                var sw2 = new Stopwatch();
                //ws2812b LEDs need 30 µs = 0.030 ms for each led to set its color so there is a lower minimum to the allowed refresh rate
                //receiving over serial takes it time as well and the arduino does both tasks in sequence
                //+1 ms extra safe zone
                double fastLedTime;
                // if (Controller.HardwareType == HardwareTypeEnum.AmbinoHUBV2)
                //     fastLedTime = ((192) / 3.0 * 0.030d);
                // else
                    fastLedTime = ((bufferLength - _messagePreamble.Length - 6) / 3.0 * 0.030d);
                var serialTransferTime = bufferLength * 10.0 * 1000.0 / baudRate;
                var minTimespan = (byte)(fastLedTime + serialTransferTime + 1);
                sw.Stop();
                int extra = 0;
                if (sw.ElapsedMilliseconds < 20)
                {
                    extra = (int)(20 - sw.ElapsedMilliseconds);
                }

                if (extra > 0)
                    Thread.Sleep(extra);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Device is removed or malfunction: " + _serialPort.PortName);
            // wait device to recover
            for (int i = 0; i < 5; i++)
            {
                Log.Warning("Waiting for device to recover!!!");
                Thread.Sleep(1000);
            }

            ControllerDisconnected?.Invoke(this.Controller);
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                }
                catch (Exception ex2)
                {
                    ///
                    Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++)
                    {
                        Log.Warning("Waiting for device to recover!!!");
                        Thread.Sleep(1000);
                    }
                }
            }

            _serialPort?.Dispose();

            //allow the system some time to recover
            Thread.Sleep(1000);
            Stop();
        }
        finally
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                Log.Information("SerialPort Disposed!");
            }
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
        }
    }
}