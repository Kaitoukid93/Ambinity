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
    public List<string> PortInUse { get; set; }

    public SerialControllerDiscoveryService()
    {
        _serialControllerHelpers = new SerialControllerHelpers();
        PortInUse = [];
    }

    public bool IsRunning => _workerThread != null && _workerThread.IsAlive;
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
        // #if DEBUG
        //         _workerThread = new Thread(() => RunDebug())
        //         {
        //             Name = "Device Discovery",
        //             IsBackground = true,
        //             Priority = ThreadPriority.BelowNormal
        //         };
        // #else

        _workerThread = new Thread(() => Run(_cancellationTokenSource.Token))
        {
            Name = "Device Discovery",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        // #endif
        _workerThread.Start();
    }

    // public void Stop()
    // {
    //     if (_workerThread == null) return;
    //     _cancellationTokenSource?.Cancel();
    //     _cancellationTokenSource = null;
    // }
    private async void RunDebug()
    {
        int count = 0;

        try
        {
            //get the list of new devices for every second
            // new device contains serial and openrgb devices ( Wled devices in the future)
            AddDummyController(HardwareTypeEnum.AmbinoBasic);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.AmbinoFanHub);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.AmbinoHUBV3);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.AmbinoEDGE);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.Dram);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.Motherboard);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.Keyboard);
            await Task.Delay(5000);
            AddDummyController(HardwareTypeEnum.Mouse);
            await Task.Delay(5000);


        }
        catch (Exception ex)
        {
            Log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
        }

        //check once a second for updates
        await Task.Delay(TimeSpan.FromSeconds(1));

    }
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
    /// <summary>
    /// add dummy controller for testing canvas
    /// </summary>
    /// <returns></returns>
    private void AddDummyController(HardwareTypeEnum type)
    {
        var controller = new SerialController();
        controller.Name = "Dummy" + type;
        controller.SerialNumber = type + "123456";
        controller.SerialPort = "COM1" + type;
        controller.FirmwareVersion = "1.0.1";
        controller.HardwareVersion = "1.0.1";
        controller.HardwareType = type;
        Dispatcher.UIThread.Invoke(() => { NewDevicesFound?.Invoke(controller); });
    }
    public async Task ScanSerialDevice()
    {
        // List of known Ambino device VID/PID pairs
        var knownDevices = new List<(string vid, string pid)>
        {
            ("1209", "c550"), // CH55X
            ("1A86", "7522"), // CH340
            ("239A", "CAFE")  // Ada
        };

        var ports = new HashSet<string>();
        foreach (var (vid, pid) in knownDevices)
        {
            var foundPorts = SerialControllerEnumerator.GetSerialPortByID(vid, pid);
            foreach (var port in foundPorts)
            {
                ports.Add(port);
            }
        }

        if (ports.Count == 0)
        {
            Log.Warning("No Compatible Device Detected");
            return;
        }

        // Filter out ports already in use
        var availablePorts = ports.Except(PortInUse).ToList();
        if (availablePorts.Count == 0)
        {
            //Log.Information("All detected ports are already in use.");
            return;
        }

        var validPorts = new List<string>();
        foreach (var port in availablePorts)
        {
            using (var serialPort = new SerialPort(port, 1000000))
            {
                serialPort.ReadTimeout = 5000;
                serialPort.WriteTimeout = 1000;
                serialPort.DtrEnable = true;
                try
                {
                    serialPort.Open();
                    await Task.Delay(1000); // Give device time to initialize
                    serialPort.Close();
                    validPorts.Add(port);
                }
                catch (Exception ex)
                {
                    Log.Warning($"Port {port} is not available: {ex.Message}");
                    if (!PortInUse.Contains(port))
                    {
                        PortInUse.Add(port); // Add to in-use list if it fails to open
                    }

                }
            }
        }

        if (validPorts.Count == 0)
        {
            Log.Warning("No valid serial ports found after filtering.");
            return;
        }

        // Process all valid ports one by one
        foreach (var selectedPort in validPorts)
        {
            await Task.Delay(500); // Wait for device to be ready

            string deviceName = null;
            string deviceID = null;
            string deviceFirmware = null;
            string deviceHardware = null;
            int deviceHWL = 0;
            HardwareTypeEnum hardwareType = HardwareTypeEnum.Unknown;
            var result = await Task.Run(() => _serialControllerHelpers.RefreshDeviceInfo(selectedPort,
                out deviceName,
                out deviceID,
                out deviceFirmware,
                out deviceHardware,
                out deviceHWL,
                out hardwareType));
            if (!result)
            {
                Log.Warning($"Failed to refresh device info for port {selectedPort}.");
                continue;
            }

            var controller = new SerialController
            {
                Name = deviceName,
                SerialNumber = deviceID,
                SerialPort = selectedPort,
                FirmwareVersion = deviceFirmware,
                HardwareVersion = deviceHardware,
                HardwareType = hardwareType
            };
            Log.Information(
    "Discovered controller:\n" +
    "  Name: {Name}\n" +
    "  SerialNumber: {SerialNumber}\n" +
    "  SerialPort: {SerialPort}\n" +
    "  FirmwareVersion: {FirmwareVersion}\n" +
    "  HardwareVersion: {HardwareVersion}\n" +
    "  HardwareType: {HardwareType}",
    controller.Name,
    controller.SerialNumber,
    controller.SerialPort,
    controller.FirmwareVersion,
    controller.HardwareVersion,
    controller.HardwareType
);
            // Notify UI thread
            Dispatcher.UIThread.Invoke(() => { NewDevicesFound?.Invoke(controller); });
        }
    }
}
