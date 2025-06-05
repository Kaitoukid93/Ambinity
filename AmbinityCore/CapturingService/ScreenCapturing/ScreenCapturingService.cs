using System.ComponentModel;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CapturingService;

public class ScreenCapturingService : ICapturingService
{
    public ScreenCapturingService(GeneralSettingsManager settingsManager)
    {
        _generalSettings = settingsManager.Settings;
        IsEnabled = _generalSettings.EnableScreenCapture;
        Init();
    }


    public bool IsEnabled { get; private set; }
    public event Action<int> FrameUpdated;

    private IScreenCaptureService? _screenCaptureService;
    private CancellationTokenSource _cancellationTokenSource = new();
    private List<IScreenCapture>? _screenCaptures { get; set; }
    private bool _disposed { get; set; }
    public List<Display> AvailableScreens => _availableScreen;
    private List<Display> _availableScreen;
    private int _userCount;
    private readonly IGeneralSettings _generalSettings;

    public void Init()
    {
        if (!IsEnabled)
            return;
        _screenCaptureService?.Dispose();
#if MACOS
        _screenCaptureService = new SCKScreenCaptureService();
#else
        _screenCaptureService = new DX11ScreenCaptureService();
#endif
        var graphicsCards = new GraphicsCard();
        _availableScreen = _screenCaptureService.GetDisplays(graphicsCards).ToList();
        _screenCaptures = new List<IScreenCapture>();

        // Cancel any previous capture threads
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        foreach (var display in _availableScreen)
        {
            var screenCapture = _screenCaptureService.GetScreenCapture(display);
            _screenCaptures.Add(screenCapture);
        }

        foreach (var screenCapture in _screenCaptures)
        {
            var thread = new Thread(() => CaptureScreen(screenCapture, _cancellationTokenSource.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "capture"
            };
            thread.Start();
        }
    }

    public void CaptureScreen(IScreenCapture screenCapture, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //call render from engine
                if (_userCount > 0)
                {
                    screenCapture.CaptureScreen();
                    FrameUpdated?.Invoke(screenCapture.Display.Index);
                    Thread.Sleep(1000 / 30);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
        finally
        {
            Dispose();
        }
    }

    public IScreenCapture GetScreenCapture(int index)
    {
        if (_screenCaptures == null)
            return null;
        if (index >= _screenCaptures.Count)
        {
            Log.Error("Display does not exist");
            return null;
        }

        return _screenCaptures[index];
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
        // Cancel all capture threads
        _cancellationTokenSource?.Cancel();

        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (_screenCaptures == null)
        {
            return;
        }

        if (disposing)
        {
            foreach (var capture in _screenCaptures)
            {
                capture?.Dispose();
            }

            _screenCaptureService?.Dispose();
        }
        _screenCaptureService = null;
        _screenCaptures = null;

        _disposed = true;
    }
}
