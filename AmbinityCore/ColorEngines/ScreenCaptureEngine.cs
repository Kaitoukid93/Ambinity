using AmbinityCore.CapturingService;
using AmbinityCore.Models.Graphic;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.LightingEngines;

public class ScreenCaptureEngine : IColorEngine
{
    public ScreenCaptureEngine(FrameBuffer buffer, CapturingServiceProvider capturingServiceProvider)
    {
        CaptureType = CapturingType.ScreenCapture;
        _capturingServiceProvider = capturingServiceProvider;

    }

    private LightingZone _zone;
    private FrameBuffer _buffer;
    private CapturingServiceProvider _capturingServiceProvider;
    private IScreenCapture _screenCapture;
    private ICaptureZone _captureZone;
    public void Init(LightingZone zone)
    {
        _zone = zone;
        var zoneConfig = (ScreenCaptureConfiguration)_zone.LightingConfiguration;
        //get screen index this zone desired
        var capturingService =(ScreenCapturingService)_capturingServiceProvider.GetCapturingService(this);
        var displayIndex = zoneConfig.DisplayIndex;
        _screenCapture = capturingService.GetScreenCapture(displayIndex);
        if (_screenCapture == null)
        {
            Log.Error("Screen Capture Engine Init Failed");
            return;
        }
        
        var left = zoneConfig.ScreenCaptureArea.Left * _screenCapture.Display.Width;
        var top = zoneConfig.ScreenCaptureArea.Top * _screenCapture.Display.Height;
        var width = zoneConfig.ScreenCaptureArea.Width*_screenCapture.Display.Width;
        var height = zoneConfig.ScreenCaptureArea.Height*_screenCapture.Display.Height;
        //calculating downscale level to get exact size of the image
        // first calculate desire desktop size
        var ratioX = width / _zone.Width;
        var ratioY = height / _zone.Height;
        var ratio=Math.Max(ratioX, ratioY);
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
        else if (ratio >= 8 )
        {
            downscaleLevel = 3;
            convertedRatio = 8;
        }
        
        _captureZone = _screenCapture.RegisterCaptureZone((int)left, (int)top, (int)_zone.Width* convertedRatio,
            (int)_zone.Height*convertedRatio, downscaleLevel: downscaleLevel);
    }
    public void Render()
    {
        
    }

    public CapturingType CaptureType { get; set; }
}