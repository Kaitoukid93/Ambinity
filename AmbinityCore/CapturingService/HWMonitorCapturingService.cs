using AmbinityCore.CapturingService.HWMonitorCapturing;
using AmbinityCore.DataBase;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Motherboard;
using MathNet.Numerics.Statistics;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CapturingService;

public class HWMonitorCapturingService : ICapturingService
{
    public event Action DataUpdate;

    public HWMonitorCapturingService(GeneralSettingsManager settingsManager)
    {
        IsEnabled = settingsManager.Settings.EnableHWMonitor&& !OperatingSystem.IsMacOS();
        _hardwares = new List<IHardware>();
        _fanSpeedSensors = new List<ISensor>();
        _fanControlSensors = new List<ISensor>();
        Init();
    }
    public bool IsEnabled { get; private set; }
    public event Action<int> FrameUpdated;
    private Computer _computer;
    private UpdateVisitor _updateVisitor;
    private List<IHardware> _hardwares;
    private List<ISensor> _fanControlSensors;
    public List<ISensor> FanControlSensors => _fanControlSensors;
    private List<ISensor> _fanSpeedSensors;
    private Motherboard _motherboard;
    private HWMonitorCaptureDataBuffer _buffer;
    public HWMonitorCaptureDataBuffer Buffer => _buffer;
    private double[] _sensorValue = new double[3];
    private bool _disposed { get; set; }
    private int _userCount = 0;
    private CancellationTokenSource _cancellationTokenSource;

    public bool IsAvailable { get; set; }

    public void Init()
    {
        if(!IsEnabled)
            return;
        _computer = new LibreHardwareMonitor.Hardware.Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };
        _computer.Open();
        _computer.Accept(_updateVisitor = new UpdateVisitor());
        _cancellationTokenSource = new CancellationTokenSource();
        //logging purpose
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Motherboard)
                _motherboard = hardware as Motherboard;
            _hardwares.Add(hardware);
            Log.Information(hardware.HardwareType.ToString() + " " + hardware.Name);
        }

        if (_motherboard is null)
        {
            Log.Warning("No motherboard found");
            return;
        }

        if (_motherboard.SubHardware.Length > 0) // check if any subhardware in motherboard
        {
            foreach (var hardware in _motherboard.SubHardware)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Fan) // fan speed sensors
                    {
                        _fanSpeedSensors.Add(sensor);
                        Log.Information(sensor.SensorType.ToString() + " " + sensor.Name);
                    }

                    if (sensor.SensorType == SensorType.Control) // fan speed sensors
                    {
                        _fanControlSensors.Add(sensor);
                        Log.Information(sensor.SensorType.ToString() + " " + sensor.Name);
                    }
                }
            }
        }

        if (_fanControlSensors.Count == 0)
        {
            Log.Warning("No fan speed sensor found");
            return;
        }

        IsAvailable = true;
        _buffer = new HWMonitorCaptureDataBuffer(_fanControlSensors.Count);
        var thread = new Thread(() => Capture(_cancellationTokenSource.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "capture"
        };
        thread.Start();
    }

    public void Capture(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            //call render from engine
            if (_userCount > 0)
            {
                _computer.Accept(_updateVisitor);
                for (var i = 0; i < _fanSpeedSensors.Count; i++)
                {
                    if (_fanSpeedSensors[i].Value == double.NaN) // this is speed target control but header is empty
                    {
                        _fanControlSensors.RemoveAt(i);
                    }
                }
                foreach (var sensor in _fanControlSensors)
                {
                    if (sensor.Value.HasValue)
                    {
                        _sensorValue[0] = sensor.Value.Value;
                        _sensorValue[1] = sensor.Min.Value;
                        _sensorValue[2] = sensor.Max.Value;
                        _buffer.Put(sensor.Index, _sensorValue);
                      //  Log.Information(sensor.Name + ": " + "Value:" + _sensorValue[0] + ", Min:" + _sensorValue[1] +
                                      //  ", Max:" + _sensorValue[2]);
                    }
                }

                DataUpdate?.Invoke();
                //get median value??
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(1000);
            }
        }
    }

    public void RegisterUse()
    {
        _userCount++;
    }

    public void UnregisterUse()
    {
        if (_userCount > 0)
            _userCount--;
    }

    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _computer?.Close();
        _disposed = true;
    }
}
