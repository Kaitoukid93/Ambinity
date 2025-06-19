using System.ComponentModel;
using System.IO.Ports;
using System.Text.RegularExpressions;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.GeneralSetting;
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

    private IGeneralSettings _settings;
    private SerialControllerHelpers _serialControllerHelpers;
    public List<string> PortInUse { get; set; }

    public SerialControllerDiscoveryService(GeneralSettingsManager generalSettingsManager)
    {
        _settings = generalSettingsManager.Settings;
        _settings.PropertyChanged += OnSettingsChanged;
        _serialControllerHelpers = new SerialControllerHelpers();
        PortInUse = [];
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGeneralSettings.AutoScanNewDevices))
        {
            if (_settings.AutoScanNewDevices)
            {
                Start();
            }
            else
            {
                Hold();
            }
        }
    }

    public bool IsRunning => _workerThread != null && _workerThread.IsAlive;
    private Thread _workerThread;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _onHold;

    public void Hold()
    {
        _onHold = true;
        _cancellationTokenSource?.Cancel();
        _workerThread = null;
    }

    public async Task Resume(int afterSeconds = 0)
    {
        await Task.Delay(afterSeconds * 1000);
        _onHold = false;
        Start();
    }

    public void Start()
    {
        if (!_settings.AutoScanNewDevices)
            return;
        if (IsRunning)
            return;

        _cancellationTokenSource = new CancellationTokenSource();

        _workerThread = new Thread(() => Run(_cancellationTokenSource.Token))
        {
            Name = "Device Discovery",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        _workerThread.Start();
    }

    private async void Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!_onHold)
                {
                    await ScanSerialDevice(token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"error when scanning devices : {ex.GetType().FullName}: {ex.Message}");
            }

            // Check once a second for updates, but exit early if cancelled
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
    private int _noDeviceWarningCount = 0;
    private const int MaxNoDeviceWarnings = 5;
    /// <summary>
    /// add a controller manually
    /// </summary>
    /// <returns></returns>
    public void ManuallyAddController(SerialController controller)
    {
        controller.FirmwareVersion = "1.0.1";
        controller.HardwareVersion = "1.0.1";
        Dispatcher.UIThread.Invoke(() => { NewDevicesFound?.Invoke(controller); });
    }

    public async Task ScanSerialDevice(CancellationToken token)
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
            if (_noDeviceWarningCount < MaxNoDeviceWarnings)
            {
                Log.Warning("No Compatible Device Detected");
                _noDeviceWarningCount++;
            }
            return;
        }
        else
        {
            _noDeviceWarningCount = 0;
        }

        // Filter out ports already in use
        var availablePorts = ports.Except(PortInUse).ToList();
        if (availablePorts.Count == 0)
        {
            return;
        }

        var validPorts = new List<string>();
        foreach (var port in availablePorts)
        {
            token.ThrowIfCancellationRequested();
            using (var serialPort = new SerialPort(port, 1000000))
            {
                serialPort.ReadTimeout = 5000;
                serialPort.WriteTimeout = 1000;
                serialPort.DtrEnable = true;
                try
                {
                    serialPort.Open();
                    await Task.Delay(1000, token); // Give device time to initialize
                    serialPort.Close();
                    validPorts.Add(port);
                }
                catch (OperationCanceledException)
                {
                    // Propagate cancellation
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Warning($"Port {port} is not available: {ex.Message}");
                    if (ex is UnauthorizedAccessException)
                    {
                        if (!PortInUse.Contains(port))
                        {
                            PortInUse.Add(port);
                        }
                    }
                    else
                    {
                        Log.Error($"Unexpected error while checking port {port}: {ex}");
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
            token.ThrowIfCancellationRequested();
            await Task.Delay(500, token); // Wait for device to be ready

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
                out hardwareType), token);
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
