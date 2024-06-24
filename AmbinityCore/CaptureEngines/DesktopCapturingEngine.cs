using System.Diagnostics;
using System;
using System.Threading.Tasks;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using Avalonia.Threading;
using Polly;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CaptureEngines;

public class DesktopCapturingEngine : ICaptureEngine
{
    public event Action<ICaptureZone, int> ScreenUpdated;

    public DesktopCapturingEngine(GeneralSettingsManager generalSettings)
    {
        _generalSettings = generalSettings.Settings;
        _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryForever(ProvideDelayDuration);
        RefreshCapturingState();
    }

    #region private field

    private List<Thread> _workerThreads;

    private enum RunningState
    {
        Capturing,
        Waiting,
        Canceling
    };

    private RunningState _state = RunningState.Canceling;
    private IGeneralSettings _generalSettings;
    private readonly Policy _retryPolicy;

    #endregion

    #region public properties

    public List<ICaptureZone> AvailableDesktop { get; set; }
    private IScreenCaptureService _captureService;
    public bool IsRunning { get; private set; } = false;
    public ByteFrame[] Frames { get; set; }
    public ByteFrame Frame { get; set; }
    private CancellationTokenSource _cancellationTokenSource;
    public object Lock { get; } = new object();
    private int _serviceRequired;

    public int ServiceRequired
    {
        get { return _serviceRequired; }
        set
        {
            if ((_serviceRequired == 0 && value == 1) || (_serviceRequired == 1 && value == 0))
            {
                _serviceRequired = value;
                RefreshCapturingState();
            }

            _serviceRequired = value;
        }
    }

    #endregion

    public void RefreshCapturingState()
    {
        var isRunning = _state != RunningState.Canceling;
        var shouldBeRunning = true;
        _captureService = new DX11ScreenCaptureService();
        IEnumerable<GraphicsCard> graphicsCards = _captureService.GetGraphicsCards();

        IEnumerable<Display> displays = _captureService.GetDisplays(graphicsCards.First());
        //create separate thread for each display
        AvailableDesktop = new List<ICaptureZone>();

        if (isRunning && !shouldBeRunning)
        {
            //stop it!
            Log.Information("DesktopFrameWGC is disabled,waiting for instruction");
            var index = 0;
            _captureService?.Dispose();
            _state = RunningState.Waiting;
        }
        // this is start sign
        else if (!isRunning && shouldBeRunning)
        {
            _workerThreads = new List<Thread>();
            _cancellationTokenSource = new CancellationTokenSource();
            Log.Information("starting WCG");
            var index = 0;
            foreach (var display in displays)
            {
                IScreenCapture screenCapture = _captureService.GetScreenCapture(display);
                ICaptureZone fullscreen = screenCapture.RegisterCaptureZone(0, 0, screenCapture.Display.Width,
                    screenCapture.Display.Height, downscaleLevel: 3);
                AvailableDesktop.Add(fullscreen);
                var workerThread =
                    new Thread(() => Run(screenCapture, index, fullscreen, _cancellationTokenSource.Token))
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.BelowNormal,
                        Name = "ScreenCapture .Net" + display.DeviceName
                    };
                _state = RunningState.Capturing;
                workerThread.Start();
                _workerThreads.Add(workerThread);
            }
        }
    }


    private TimeSpan ProvideDelayDuration(int index)
    {
        if (index < 10)
        {
            return TimeSpan.FromMilliseconds(100);
        }

        if (index < 10 + 256)
        {
            //steps where there is also led dimming

            return TimeSpan.FromMilliseconds(5000d / 256);
        }

        return TimeSpan.FromMilliseconds(1000);
    }

    private void Run(IScreenCapture capture, int index, ICaptureZone zone, CancellationToken token)
    {
        Log.Information("WCG is running for screen :" + zone.Display.DeviceName);
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (_state == RunningState.Capturing)
                {
                    capture.CaptureScreen();
                    var frameTime = Stopwatch.StartNew();
                    Dispatcher.UIThread.InvokeAsync(()=>ScreenUpdated?.Invoke(zone, index));
                    

                    var minFrameTimeInMs = 1000 / 30;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, ToString());
        }
    }

    public void Stop()
    {
        Log.Information("Stop called for WCG");
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        _state = RunningState.Canceling;
        IsRunning = false;
        for (var i = 0; i < _workerThreads.Count(); i++)
        {
            if (_workerThreads[i] == null) return;
            //_captures[i]?.Dispose();
            GC.Collect();
            _workerThreads[i]?.Join();
            _workerThreads[i] = null;
        }
    }

    public void StopCapture(int screenIndex)
    {
        _captureService.Dispose();
        Log.Information("Dispose called");
    }
}