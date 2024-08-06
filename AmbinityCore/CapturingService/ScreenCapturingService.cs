using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CapturingService;

public class ScreenCapturingService : ICapturingService
{
    public ScreenCapturingService()
    {
        Init();
    }


    private IScreenCaptureService _screenCaptureService;
    private List<IScreenCapture> _screenCaptures { get; set; }
    private bool _disposed { get; set; }

    public void Init()
    {
        _screenCaptureService?.Dispose();
        _screenCaptureService ??= new DX11ScreenCaptureService();
        IEnumerable<GraphicsCard> graphicsCards = _screenCaptureService.GetGraphicsCards();
        IEnumerable<Display> displays = _screenCaptureService.GetDisplays(graphicsCards.First());
        _screenCaptures = new List<IScreenCapture>();
        foreach (var display in displays)
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
        while (true)
        {
            //call render from engine

            // _paletteEngine.Render(zone, _imageBuffer, colorBank)
            screenCapture.CaptureScreen();
            Thread.Sleep(1000 / 30);

        }
    }
    public IScreenCapture GetScreenCapture(int index)
    {
        if (index >= _screenCaptures.Count)
        {
            Log.Error("Display does not exist");
            return null;
        }

        return _screenCaptures[index];
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