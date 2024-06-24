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
    /// <summary>
    /// this class contains long-running task to get all display available and return
    /// as ICaptureZone (both full screen)
    /// </summary>
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

    public int ServiceRequired { get; set; }

    #endregion

    public void RefreshCapturingState()
    {
        var isRunning = _state != RunningState.Canceling;
        var shouldBeRunning = ServiceRequired > 0;
        _captureService ??= new DX11ScreenCaptureService();

        switch (isRunning)
        {
            case true when !shouldBeRunning:
            {
                //send loop to waiting state
                Log.Information("DesktopFrameWGC is disabled,waiting for instruction");
                _state = RunningState.Waiting;
                break;
            }
            // re-create new instance
            case false when shouldBeRunning:
            {
                var graphicsCards = _captureService.GetGraphicsCards();
                var displays = _captureService.GetDisplays(graphicsCards.First());
                //create separate thread for each display
                AvailableDesktop = [];
                _workerThreads = [];
                _cancellationTokenSource = new CancellationTokenSource();
                Log.Information("starting WCG");
                var index = 0;
                foreach (var display in displays)
                {
                    var screenCapture = _captureService.GetScreenCapture(display);
                    var fullscreen = screenCapture.RegisterCaptureZone(0, 0, screenCapture.Display.Width,
                        screenCapture.Display.Height, downscaleLevel: 3);
                    AvailableDesktop.Add(fullscreen);
                    var workerThread =
                        new Thread(() => Run(screenCapture, index++, fullscreen, _cancellationTokenSource.Token))
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.BelowNormal,
                            Name = "ScreenCapture .Net" + display.DeviceName
                        };
                    _state = RunningState.Capturing;
                    workerThread.Start();
                    _workerThreads.Add(workerThread);
                }

                break;
            }
            //simply just enable the loop
            case true when shouldBeRunning:
            {
                _state = RunningState.Capturing;
                break;
            }
        }
    }


    private TimeSpan ProvideDelayDuration(int index)
    {
        return index switch
        {
            < 10 => TimeSpan.FromMilliseconds(100),
            < 10 + 256 => TimeSpan.FromMilliseconds(5000d / 256),
            _ => TimeSpan.FromMilliseconds(1000)
        };
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
                    Dispatcher.UIThread.InvokeAsync(() => ScreenUpdated?.Invoke(zone, index));
                    const int minFrameTimeInMs = 1000 / 100;
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

    private void Stop()
    {
        Log.Information("Stop called for WCG");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        
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

    public void Dispose()
    {
        Stop();
        _captureService.Dispose();
        Log.Information("Dispose called");
    }
}