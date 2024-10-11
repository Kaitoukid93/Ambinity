using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.HWMonitorCapturing;
using AmbinityCore.Models.Device.Controller;
using LibreHardwareMonitor.Hardware;
using MathNet.Numerics.Statistics;
using Serilog;

namespace AmbinityCore.Models.Device;

/// <summary>
/// service for setting fan speed, this act like a lighting engine but instead of capturing bitmap,
/// it captures fan speed from HWMonitor service
/// </summary>
public class FanOutputService
{
    private readonly HWMonitorCapturingService _hwMonitorCapturingService;
    private FanOutput _fanOuput;
    private List<double> _speeds;
    private HWMonitorCaptureDataBuffer _buffer;

    public FanOutputService(CapturingServiceProvider capturingServiceProvider, FanOutput output)
    {
        _fanOuput = output;
        _hwMonitorCapturingService =
            (HWMonitorCapturingService)capturingServiceProvider.GetCapturingService(CapturingType.HWCapture);
        _speeds = new List<double>();
    }

    private CancellationTokenSource _cancellationTokenSource;
    private Thread _workerThread;

    public void Init()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _hwMonitorCapturingService.RegisterUse();
        _buffer = _hwMonitorCapturingService.Buffer;
        if (_buffer == null)
            _fanOuput.ControlMode = FanControlModeEnum.Fixed;
        _workerThread = new Thread(() => Run(_cancellationTokenSource.Token))
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal,
            Name = "FanOutputService"
        };
        _workerThread.Start();
    }

    private void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CalculateSpeed(_fanOuput);
            Thread.Sleep(1000);
        }
    }

    private void CalculateSpeed(FanOutput output)
    {
        int speed = 80;
        if (output.ControlMode == FanControlModeEnum.Fixed)
            output.Speed = output.FixedSpeed;
        else if (output.ControlMode == FanControlModeEnum.Adaptive)
        {
            //only get valid value
            for (int i = 0; i < _buffer.Size; i++)
            {
                var value = _buffer.GetValue(i, 0);
                var min = _buffer.GetValue(i, 1);
                var max = _buffer.GetValue(i, 2);
                if (value == min && value == max && Math.Round(value) == 100)
                    continue;
                _speeds.Add(value);
            }

            if (_speeds.Count > 0)
                output.Speed = (int)_speeds.Median();
           // Log.Information(output.Name + " Speed: " + output.Speed);
        }
    }

    public void Dispose()
    {
        _hwMonitorCapturingService.UnregisterUse();
        GC.Collect();
    }
}