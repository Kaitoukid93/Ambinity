using System.Buffers;
using System.CodeDom;
using System.Diagnostics;
using AmbinityCore.Enums;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.OpenRGB;
using OpenRGB.NET;
using Serilog;

namespace AmbinityCore.DataStream;

public class OpenRGBStream : IDataStream
{
    public OpenRGBStream(IController controller, AmbinityOpenRGBClient client)
    {
        Controller = controller;
        _client = client;
    }

    public event Action<IController>? ControllerDisconnected;
    private AmbinityOpenRGBClient _client;
    public bool IsRunning => _workerThread != null && _workerThread.IsAlive;
    public string ID { get; set; }
    private int _deviceIndex;
    private bool _isDeviceValid;
    public string Port { get; private set; }
    private CancellationTokenSource _cancellationTokenSource;

    private Thread _workerThread;
    private bool _isInitializing;

    public void Init()
    {
        if (_isInitializing)
            return;
        _isInitializing = true;
        ID = Controller.SerialNumber;
        Port = Controller.SerialPort;
        Controller.WorkingStateEnum = ControllerWorkingStateEnum.Normal;
        // _hasPWMCOntroller = Controller.FanController != null;
        Start();
        _isInitializing = false;
    }

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
    /// Refresh incase of index change
    /// </summary>
    public void Refresh()
    {
        GetDeviceIndex();
    }

    private void GetDeviceIndex()
    {
        _isDeviceValid = false;
        lock (_client.Lock)
        {
            for (var i = 0; i < _client.OpenRGBClient.GetControllerCount(); i++)
            {
                var device = _client.OpenRGBClient.GetControllerData(i);
                var deviceName = device.Name.ToValidFileName();
                if (Controller.Name + Controller.SerialPort ==
                    deviceName.ToValidFileName() + device.Location.ToValidFileName())
                {
                    //so we're at i
                    _deviceIndex = i;
                    _isDeviceValid = true;
                    break;
                }
            }
        }
    }

    private Color[] GetOutputStream(int id)
    {
        Color[] outputStream;
        var output = Controller.LedController.Outputs[id];
        var devices = Controller.LedController.Outputs[id].Devices;
        int ledCount = 0;
        foreach (var device in devices)
        {
            ledCount += device.Leds.Count;
        }

        double brightnessCap = output.Brightness / 255d;
        outputStream = new Color[ledCount];
        DimLED();
        int counter = 0;
        foreach (var device in devices)
        {
            lock (device.Lock)
            {
                if (device.Leds.Count == 0) //this could be PID has removed all items add 1 dummy
                {
                    outputStream[counter++] = new Color(0, 0, 0);
                }
                else
                {
                    var rgbOrder = device.RGBOrder;
                    foreach (AmbinityLED led in device.Leds)
                    {
                        ApplyColorWhitebalance(led.LED.Red, led.LED.Green, led.LED.Blue,
                            device.RedScale, device.GreenScale,
                            device.BlueScale,
                            out byte FinalR, out byte FinalG, out byte FinalB);
                        ReOrderSpotColor(rgbOrder, FinalR, FinalG, FinalB, out byte r, out byte g, out byte b);
                        //get data
                        outputStream[counter++] = new Color((byte)(led.LED.Red * _dimFactor * brightnessCap),
                            (byte)(led.LED.Green * _dimFactor * brightnessCap),
                            (byte)(led.LED.Blue * _dimFactor * brightnessCap));
                    }
                }
            }
        }


        return outputStream;
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

    public void Start()
    {
        if (IsRunning)
            return;
        if (!Controller.AutoConnect)
            return;
        Log.Information("Start called for OpenRGBStream");
        if (!_client.IsInitialized)
            return;
        GetDeviceIndex();
        if (!_isDeviceValid)
            return;
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
        Controller.TurnOn();
    }

    private void DoWork(object tokenObject)
    {
        try
        {
            var cancellationToken = (CancellationToken)tokenObject;
            var ledCount = _client.OpenRGBClient.GetControllerData(_deviceIndex).Leds.Count();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                //send frame data
                var outputColor = new List<Color>();
                for (var i = 0; i < Controller.LedController.Outputs.Count; i++)
                {
                    var stream = GetOutputStream(i);
                    outputColor.AddRange(stream);
                }
                //add led with same color if missing
                if (outputColor.Count < ledCount)
                {
                    var numLedMissing = ledCount - outputColor.Count;
                    var lastLEDColor = outputColor.Last();
                    for (int i = 0; i < numLedMissing; i++)
                    {
                        outputColor.Add(lastLEDColor);
                    }
                }
                lock (_client.Lock)
                {
                    if (_client.IsInitialized && _isDeviceValid)
                        _client.OpenRGBClient.UpdateLeds(_deviceIndex, outputColor.Take(ledCount).ToArray());
                }

                Thread.Sleep(1000 / 30);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            //allow the system some time to recover
            Thread.Sleep(1000);
            Stop();
        }
    }

    public async Task Stop()
    {
        if (!IsRunning)
            return;
        Controller.TurnOff();
        //wait for led to fully turn off
        await Task.Run(() => Task.Delay(1000));
        Controller.DisableTransfer();
        Log.Information("Stop called for OpenRGB Stream");
        if (_workerThread == null) return;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
    }

    public bool IsValid()
    {
        return true;
        //throw new NotImplementedException();
    }

    public IController Controller { get; set; }
}