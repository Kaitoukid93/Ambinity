using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.CapturingService;

public class ScreenCapturingService : ICapturingService
{
    public ScreenCapturingService()
    {
        
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
            _screenCaptureService?.Dispose();
        }

        _screenCaptures = null;

        _disposed = true;
    }
}