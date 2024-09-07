using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using AmbinityCore.Utils;
using Avalonia;
using Draw2D.Core.Graphic;
using HPPH;
using ScreenCapture.NET;
using Serilog;

namespace AmbinityCore.LightingEngines;

public class ScreenCaptureEngine : IColorEngine
{
    public ScreenCaptureEngine(FrameBuffer buffer, CapturingServiceProvider capturingServiceProvider,
        AmbinityDeviceRepository deviceRepository)
    {
        CaptureType = CapturingType.ScreenCapture;
        _buffer = buffer;
        _capturingServiceProvider = capturingServiceProvider;
        _deviceRepository = deviceRepository;
    }

    public LightingZone Zone => _zone;
    private LightingZone _zone;
    private FrameBuffer _buffer;
    private CapturingServiceProvider _capturingServiceProvider;
    private IScreenCapture _screenCapture;
    private ICaptureZone _captureZone;
    private byte[] _reusableRow;
    private readonly AmbinityDeviceRepository _deviceRepository;
    private List<CaptureRect> _ledRects;
    private Rect _captureZoneRect => new Rect(_captureZone.X, _captureZone.Y, _captureZone.Width, _captureZone.Height);

    public void Init(LightingZone zone)
    {
        _zone = zone;
        _zone.UpdateFrameBuffer();
        _ledRects = new List<CaptureRect>();
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
        // var ratioX = width / _zone.Width;
        // var ratioY = height / _zone.Height;
        // var ratio = Math.Min(ratioX, ratioY);
        // var convertedRatio = 1;
        // int downscaleLevel = 0;
        // if (ratio < 2)
        // {
        //     downscaleLevel = 0;
        //     convertedRatio = 1;
        // }
        //
        // else if (ratio >= 2 && ratio < 4)
        // {
        //     downscaleLevel = 1;
        //     convertedRatio = 2;
        // }
        //
        // else if (ratio >= 4 && ratio < 8)
        // {
        //     downscaleLevel = 2;
        //     convertedRatio = 4;
        // }
        // else if (ratio >= 8)
        // {
        //     downscaleLevel = 3;
        //     convertedRatio = 8;
        // }

        try
        {
            _captureZone = _screenCapture.RegisterCaptureZone((int)left, (int)top, (int)width,
                (int)height, downscaleLevel: 3);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }

        _reusableRow = new byte[(int)_zone.Width * 4];
        UpdatePixelData();
    }

    private void UpdatePixelData()
    {
        foreach (var device in _deviceRepository.Devices)
        {
            var rect = _zone.ZoneBound.Intersect(device.Bound);
            if (rect == default)
                continue;
            foreach (var led in device.Leds)
            {
                var intersect = _zone.ZoneBound.Intersect(led.TransformedRect);
                if (intersect == default)
                    continue;
                var translatedRect = RectCalculation.TranslateRect(led.TransformedRect, _zone.Bound, _captureZoneRect);
                _ledRects.Add(new CaptureRect(led.TransformedRect,translatedRect));
            }
        }
    }

    public void Render()
    {
        using (_captureZone.Lock())
        {
            IImage image = _captureZone.Image;
            Span<byte> row = _reusableRow;
            //render whole image
            
            // for (int i = 0; i < image.Height; i++)
            // {
            //     image.Rows[i].CopyTo(row);
            //
            //     int start = (_buffer.FrameWidth * 4) * (i + (int)_zone.Y) + (int)_zone.X * 4;
            //     int start2 = (_zone.Buffer.FrameWidth * 4) * i;
            //     Array.Copy(_reusableRow, 0, _buffer.PixelData, start, _reusableRow.Length);
            // }

            //render rect only
            foreach (var rect in _ledRects)
            {
                //translate rect to image coordinate system
              //  var translatedRect = RectCalculation.TranslateRect(rect, _zone.Bound, _captureZoneRect);
                IImage subImage = image[(int)rect.TranslatedRect.X, (int)rect.TranslatedRect.Y, (int)rect.TranslatedRect.Width,
                    (int)rect.TranslatedRect.Height];
                //render sub image at led rect position
                var col = subImage.Average();
                ColorComputing.SetBlockColor(_buffer,rect.OriginalRect,col.R,col.G,col.B);
            }
        }
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        if (_captureZone != null)
            _screenCapture.UnregisterCaptureZone(_captureZone);
        GC.Collect();
    }

    public CapturingType CaptureType { get; set; }
}