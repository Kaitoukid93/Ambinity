using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CapturingService;

public class ScreenCapturingService : ICapturingService
{
    public ScreenCapturingService()
    {
        Init();
    }

    public event Action<int> FrameUpdated;

    private IScreenCaptureService _screenCaptureService;
    private List<IScreenCapture> _screenCaptures { get; set; }
    private bool _disposed { get; set; }
    public List<Display> AvailableScreens => _availableScreen;
    private List<Display> _availableScreen;
    private int _userCount;

    public void Init()
    {
        _screenCaptureService?.Dispose();
        _screenCaptureService ??= new DX11ScreenCaptureService();
        IEnumerable<GraphicsCard> graphicsCards = _screenCaptureService.GetGraphicsCards();
        _availableScreen = _screenCaptureService.GetDisplays(graphicsCards.First()).ToList();
        _screenCaptures = new List<IScreenCapture>();
        foreach (var display in _availableScreen)
        {
            var screenCapture = _screenCaptureService.GetScreenCapture(display);
            _screenCaptures.Add(screenCapture);
        }

        foreach (var screenCapture in _screenCaptures)
        {
            var thread = new Thread(() => CaptureScreen(screenCapture))
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "capture"
            };
            thread.Start();
        }
    }

    public void CaptureScreen(IScreenCapture screenCapture)
    {
        try
        {
            while (true)
            {
                //call render from engine
                if (_userCount > 0)
                {
                    // _paletteEngine.Render(zone, _imageBuffer, colorBank)

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

        if (disposing)
        {
            foreach (var capture in _screenCaptures)
            {
                capture?.Dispose();
            }

            _screenCaptureService?.Dispose();
        }

        _screenCaptures = null;

        _disposed = true;
    }
}