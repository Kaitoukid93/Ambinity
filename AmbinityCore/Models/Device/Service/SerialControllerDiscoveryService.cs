using System.IO.Ports;
using System.Text.RegularExpressions;
using AmbinityCore.Models.Device.Controller;
using Avalonia.Threading;
using Microsoft.Win32;
using Serilog;

namespace AmbinityCore.Models.Device.Service;

public class SerialControllerDiscoveryService
{
    /// <summary>
    /// Continuously searching for new device in background
    /// </summary>
    public event Action<SerialController> NewDevicesFound;

    public event Action<string> NewComportDetected;

    private SerialControllerHelpers _serialControllerHelpers;

    public SerialControllerDiscoveryService()
    {
        _serialControllerHelpers = new SerialControllerHelpers();
    }

    public bool IsRunning  => _workerThread != null && _workerThread.IsAlive;
    private Thread _workerThread;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _onHold;

    public void Hold()
    {
        _onHold = true;
    }

    public async Task Resume(int afterSeconds = 0)
    {
        await Task.Delay(afterSeconds * 1000);
        _onHold = false;
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _workerThread = new Thread(() => Run(_cancellationTokenSource.Token))
        {
            Name = "Device Discovery",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        _workerThread.Start();
    }

    // public void Stop()
    // {
    //     if (_workerThread == null) return;
    //     _cancellationTokenSource?.Cancel();
    //     _cancellationTokenSource = null;
    // }

    private async void Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                //get the list of new devices for every second
                // new device contains serial and openrgb devices ( Wled devices in the future)
                if (!_onHold)
                {
                    await ScanSerialDevice();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
            }

            //check once a second for updates
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public async Task ScanSerialDevice()
    {
        //these are valid PID VID used by Ambino devices
        List<string> CH55X = SerialControllerHelpers.GetComPortByID("1209", "c550");
        List<string> CH340 = SerialControllerHelpers.GetComPortByID("1A86", "7522");
        List<string> ada = SerialControllerHelpers.GetComPortByID("239A", "CAFE");
        var ports = new List<string>();
        if (CH55X.Count > 0 || CH340.Count > 0 || ada.Count > 0)
        {
            foreach (var port in CH55X)
            {
                ports.Add(port);
            }

            foreach (var port in CH340)
            {
                ports.Add(port);
            }

            foreach (var port in ada)
            {
                ports.Add(port);
            }
        }
        else
        {
            Log.Warning("No Compatible Device Detected");
        }

        var invalidDevice = new List<string>();
        foreach (var port in ports)
        {
            var _serialPort = new SerialPort(port, 1000000);
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.DtrEnable = true;
            try
            {
                _serialPort.Open();
                Thread.Sleep(1000);
                _serialPort.Close();
                _serialPort.Dispose();
            }
            catch (Exception ex)
            {
                invalidDevice.Add(port);
            }
        }

        foreach (var device in invalidDevice)
        {
            ports.Remove(device);
        }

        if (ports.Count > 0)
        {
            // Dispatcher.UIThread.Invoke(() => { NewComportDetected?.Invoke(ports.First()); });
            await Task.Delay(500);
            string deviceName = null;
            string deviceID = null;
            string deviceFirmware = null;
            string deviceHardware = null;
            int deviceHWL = 0;
            HardwareTypeEnum hardwareType = HardwareTypeEnum.Unknown;
            var result = await Task.Run(() => _serialControllerHelpers.RefreshDeviceInfo(ports.First(),
                out deviceName,
                out deviceID,
                out deviceFirmware,
                out deviceHardware,
                out deviceHWL,
                out hardwareType));
            if (!result)
                return;
            var controller = new SerialController();
            controller.Name = deviceName;
            controller.SerialNumber = deviceID;
            controller.SerialPort = ports.First();
            controller.FirmwareVersion = deviceFirmware;
            controller.HardwareVersion = deviceHardware;
            controller.HardwareType = hardwareType;

            //invoke provider
            Dispatcher.UIThread.Invoke(() => { NewDevicesFound?.Invoke(controller); });
        }
    }
}