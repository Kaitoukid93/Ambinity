using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia.Threading;
using Draw2D.Core.Graphic;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.LightingEngines;

public class ScreenCaptureEngine : IColorEngine
{
    public ScreenCaptureEngine(FrameBuffer buffer, CapturingServiceProvider capturingServiceProvider)
    {
        CaptureType = CapturingType.ScreenCapture;
        _buffer = buffer;
        _capturingServiceProvider = capturingServiceProvider;
    }

    private LightingZone _zone;
    private FrameBuffer _buffer;
    private CapturingServiceProvider _capturingServiceProvider;
    private IScreenCapture _screenCapture;
    private ICaptureZone _captureZone;
    private byte[] _reusableRow;

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _zone.UpdateFrameBuffer();
        var zoneConfig = (ScreenCaptureConfiguration)_zone.LightingConfiguration;
        //get screen index this zone desired
        var capturingService = (ScreenCapturingService)_capturingServiceProvider.GetCapturingService(this);
        var displayIndex = zoneConfig.DisplayIndex;
        _screenCapture = capturingService.GetScreenCapture(displayIndex);
        if (_screenCapture == null)
        {
            Log.Error("Screen Capture Engine Init Failed");
            return;
        }

        var left = zoneConfig.ScreenCaptureArea.RatioX * _screenCapture.Display.Width;
        var top = zoneConfig.ScreenCaptureArea.RatioY * _screenCapture.Display.Height;
        var width = zoneConfig.ScreenCaptureArea.RatioWidth * _screenCapture.Display.Width;
        var height = zoneConfig.ScreenCaptureArea.RatioHeight * _screenCapture.Display.Height;
        //calculating downscale level to get exact size of the image
        // first calculate desire desktop size
        var ratioX = width / _zone.Width;
        var ratioY = height / _zone.Height;
        var ratio = Math.Min(ratioX, ratioY);
        var convertedRatio = 1;
        int downscaleLevel = 0;
        if (ratio < 2)
        {
            downscaleLevel = 0;
            convertedRatio = 1;
        }

        else if (ratio >= 2 && ratio < 4)
        {
            downscaleLevel = 1;
            convertedRatio = 2;
        }

        else if (ratio >= 4 && ratio < 8)
        {
            downscaleLevel = 2;
            convertedRatio = 4;
        }
        else if (ratio >= 8)
        {
            downscaleLevel = 3;
            convertedRatio = 8;
        }

        try
        {
            _captureZone = _screenCapture.RegisterCaptureZone((int)left, (int)top, (int)_zone.Width * convertedRatio,
                (int)_zone.Height * convertedRatio, downscaleLevel: downscaleLevel);
        }
        catch (Exception ex)
        {
          Log.Error(ex.ToString());  
        }
        _reusableRow = new byte[(int)_zone.Width*4];
        
    }

    public void Render()
    {
        using (_captureZone.Lock())
        {
            IImage image = _captureZone.Image;
            Span<byte> row = _reusableRow;
            for (int i = 0; i < image.Height; i++)
            {
                image.Rows[i].CopyTo(row);
                
                int start = (_buffer.FrameWidth * 4) * (i + (int)_zone.Y) + (int)_zone.X * 4;
                int start2 = (_zone.Buffer.FrameWidth * 4) * i;
                Array.Copy(_reusableRow, 0, _buffer.PixelData, start, _reusableRow.Length);
                Array.Copy(_reusableRow, 0, _zone.Buffer.PixelData, start2, _reusableRow.Length);
            }
           // _zone.UpdateFrame();
          
        }
    }
    public bool IsDisposed { get; private set; }
    public void Dispose()
    {
        IsDisposed = true;
        if(_captureZone!=null)
        _screenCapture.UnregisterCaptureZone(_captureZone);
        GC.Collect();
    }
    public CapturingType CaptureType { get; set; }
}