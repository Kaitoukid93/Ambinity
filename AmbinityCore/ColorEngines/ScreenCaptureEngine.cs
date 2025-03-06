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

    private void OnDeviceListUpdated()
    {
        UpdatePixelData();
    }

    public bool IsAvailable { get; private set; } = true;
    public LightingZone Zone => _zone;
    /// <summary>
    /// SCK capture use scaling factor of it's own
    /// </summary>
    private double _displayScalingFactor = 1.0f;
    private LightingZone _zone;
    private readonly FrameBuffer _buffer;
    private readonly CapturingServiceProvider _capturingServiceProvider;
    private IScreenCapture _screenCapture;
    private ICaptureZone _captureZone;
    private byte[] _reusableRow;
    private readonly AmbinityDeviceRepository _deviceRepository;
    private List<CaptureRect> _ledRects;
    private ScreenCaptureConfiguration _config;
    private ScreenCapturingService _screenCapturingService;
    private object renderingLock = new object();
    private Rect _captureZoneRect => new Rect(_captureZone.X, _captureZone.Y, _captureZone.Width, _captureZone.Height);

    public void Init(LightingZone zone)
    {
        _screenCapturingService =
            (ScreenCapturingService)_capturingServiceProvider.GetCapturingService(this.CaptureType);
        if (!_screenCapturingService.IsEnabled)
        {
            Log.Information("ScreenCapturingService is required but plugin is not enabled");
            IsAvailable = false;
            return;
        }
        _deviceRepository.DevicesListUpdated += OnDeviceListUpdated;
        _zone = zone;
        _zone.UpdateFrameBuffer();
        _ledRects = new List<CaptureRect>();
        _config = (ScreenCaptureConfiguration)_zone.LightingConfiguration;
        _config.CaptureAreaUpdated += OnCaptureAreaUpdated;
        _screenCapturingService.RegisterUse();
        //get screen index this zone desired
        OnCaptureAreaUpdated();
    }

    private void OnCaptureAreaUpdated()
    {
        var displayIndex = _config.DisplayIndex;
        if(displayIndex < 0 || displayIndex >= _screenCapturingService.AvailableScreens.Count)
        {
            Log.Error("Invalid display index");
            displayIndex = 0;
        }
        _screenCapture = _screenCapturingService.GetScreenCapture(displayIndex);
        if (_screenCapture == null)
        {
            Log.Error("Screen Capture Engine Init Failed");
            return;
        }
#if MACOS
    if (_screenCapture is SCKScreenCapture sckScreenCapture)
        {
            _displayScalingFactor = sckScreenCapture.ScalingFactor;
        }
#endif

        var left = _config.ScreenCaptureArea.RatioX * _screenCapture.Display.Width * _displayScalingFactor;
        var top = _config.ScreenCaptureArea.RatioY * _screenCapture.Display.Height * _displayScalingFactor;
        var width = _config.ScreenCaptureArea.RatioWidth * _screenCapture.Display.Width * _displayScalingFactor;
        var height = _config.ScreenCaptureArea.RatioHeight * _screenCapture.Display.Height * _displayScalingFactor;
        if (_captureZone != null)
            _screenCapture?.UnregisterCaptureZone(_captureZone);
        try
        {
            _captureZone = _screenCapture.RegisterCaptureZone((int)left, (int)top, (int)width,
                (int)height, downscaleLevel: 1);
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            return;
        }
        UpdatePixelData();
    }

    private void UpdatePixelData()
    {
        lock (renderingLock)
        {
            lock (_deviceRepository.Lock)
            {
                foreach (var device in _deviceRepository.Devices)
                {
                    var rect = _zone.ZoneBound.Intersect(device.Bound);
                    if (rect == default)
                        continue;
                    device.TransformLeds();
                    foreach (var led in device.Leds)
                    {
                        var intersect = _zone.ZoneBound.Intersect(led.TransformedRect);
                        if (intersect != led.TransformedRect)
                            continue;
                        var translatedRect =
                            RectCalculation.TranslateRect(led.TransformedRect, _zone.Bound, _captureZoneRect);
                        _ledRects.Add(new CaptureRect(led.TransformedRect, translatedRect));
                    }
                }
            }

        }
    }

    public void Render()
    {
        if (_captureZone == null)
            return;
        using (_captureZone.Lock())
        {
            IImage image = _captureZone.Image;
            if (_reusableRow == null)
            {
                _reusableRow = new byte[image.Width * 4];
            }
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
            lock (renderingLock)
            {
                foreach (var rect in _ledRects)
                {
                    //translate rect to image coordinate system
                    //  var translatedRect = RectCalculation.TranslateRect(rect, _zone.Bound, _captureZoneRect);
                    IImage subImage = image[(int)rect.TranslatedRect.X, (int)rect.TranslatedRect.Y,
                        (int)rect.TranslatedRect.Width,
                        (int)rect.TranslatedRect.Height];
                    //render sub image at led rect position
                    var col = subImage.Average();
                    ColorComputing.SetBlockColor(_buffer, rect.OriginalRect, (byte)(col.R), (byte)(col.G),
                        (byte)(col.B));
                }
            }
        }
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        if (_captureZone != null)
        {
            try
            {
                _screenCapture.UnregisterCaptureZone(_captureZone);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return;
            }
        }

        _screenCapturingService.UnregisterUse();
        GC.SuppressFinalize(this);
    }

    public CapturingType CaptureType { get; set; }
}