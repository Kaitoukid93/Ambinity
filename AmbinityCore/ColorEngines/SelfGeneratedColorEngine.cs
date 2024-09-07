using AmbinityCore.CapturingService;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;
using Draw2D.Core.Graphic;

namespace AmbinityCore.LightingEngines;

public class SelfGeneratedColorEngine : IColorEngine
{
    private float _startIndex = 0f;
    private Color[] _colorBank;
    public LightingZone Zone => _zone;
    private LightingZone _zone;
    private FrameBuffer _buffer;
    private SelfGeneratedColorConfiguration _config;
    private Point[] _zonePoly;
    private List<Point[]> _pixelPath;
    private List<Point[]> _lineList;
    private List<Rect> _ledRects;
    private readonly AmbinityDeviceRepository _deviceRepository;
    private ColorApperance _colorApperance;
    private float _colorResolution;
    private float _colorSpeed;
    private bool _isColorMoving;
    private bool _isReverse;
    private PaletteBlend _blendMode;
    private IMotionConfiguration _motionConfig;
    private IBrightnessProvider _brightnessProvider;
    private readonly BrightnessProviderFactory _brightnessProviderFactory;
    private float[] _brightnessData;
    private object renderingLock = new object();

    public SelfGeneratedColorEngine(FrameBuffer buffer, AmbinityDeviceRepository deviceRepository,
        BrightnessProviderFactory brightnessProviderFactory)
    {
        _deviceRepository = deviceRepository;
        _brightnessProviderFactory = brightnessProviderFactory;
        _brightnessProviderFactory.DefaultDeviceChanged += UpdateBrightnessProvider;
        _deviceRepository.NewDevicesAdded += OnNewDeviceAdded;
        _buffer = buffer;
        _lineList = new List<Point[]>();
        _colorBank = new Color[1024];
    }


    private void UpdateBrightnessProvider()
    {
        lock (renderingLock)
        {
            _brightnessProvider?.Deactivate();
            _brightnessProvider = _brightnessProviderFactory.GetBrightnessProvider(_motionConfig);
            _brightnessData = new float[_lineList.Count];
        }
    }

    private void OnNewDeviceAdded()
    {
        UpdatePixelsData();
    }

    public void Render()
    {
        if (_colorBank.Length < 1)
            return;
        lock (renderingLock)
        {
            _brightnessProvider.GetBrightness(_brightnessData);
            for (int l = 0; l < _lineList.Count; l++)
            {
                var line = _lineList[l];
                //get color at this index
                int position = (int)_startIndex + (int)(l * _colorResolution);
                position %= _colorBank.Length;
                float brightness = _brightnessData[l] / 255f;
                var r = _colorBank[position].R * brightness;
                var g = _colorBank[position].G * brightness;

                var b = _colorBank[position].B * brightness;
                lock (_buffer.FrameLock)
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        ColorComputing.PlotPixel(_buffer, (int)line[i].X, (int)line[i].Y, (byte)r, (byte)g, (byte)b);
                    }
                }
            }
        }


        if (_startIndex > _colorBank.Length)
        {
            _startIndex = 0;
        }

        //increase color index if IsMoving
        if (_isColorMoving)
            _startIndex += _colorSpeed;
    }

    private List<Point[]> GetPixels(ColorApperance apperance)
    {
        var pixelist = new List<Point[]>();
        switch (apperance.Mode)
        {
            case ColorApperanceEnum.Fill:
                pixelist = GeometryHelper.GetpixelsLineFromListOfRectangleWidthAngle(_zone.ZoneBound, _ledRects,
                    apperance.Value);
                break;
            case ColorApperanceEnum.Stroke:
                pixelist = GeometryHelper.GetPixelsFromPolylineWithThickness(_zonePoly, apperance.Value);
                break;
        }

        return pixelist;
    }

    /// <summary>
    /// Initialize with zone parameters
    /// </summary>
    /// <param name="zone"></param>
    public void Init(LightingZone zone)
    {
        _zone = zone;
        // use rectangle as the default zone type
        if (_zone.Shape == null)
            _zone.Shape = ZoneShapeEnum.Rectangle;
        //get the zone lighting config
        _config = (SelfGeneratedColorConfiguration)_zone.LightingConfiguration;

        // _brightnessProvider.
        _config.ColorsUpdated += UpdateColorsData;
        _config.ColorsBehaviorUpdated += UpdateColorsBehavior;
        _config.ApperanceUpdated += UpdatePixelsData;
        _config.MotionConfigUpdated += UpdateMotionConfig;
     
        // get the zone shape data
        _zonePoly = zone.GetPoints().ToArray();
        if (_zonePoly.Length == 0)
            return;
        //get color appearance data
        _colorApperance = _config.Apperance;
        UpdatePixelsData();
        UpdateColorsData();
        UpdateColorsBehavior();
        UpdateMotionConfig();
    }

    private void UpdateMotionConfig()
    {
        _motionConfig = _config.MotionConfig;
        _motionConfig.Update += UpdateBrightnessProvider;
        UpdateBrightnessProvider();
    }
    /// <summary>
    /// update list of leds that this zone care about 
    /// </summary>
    private void UpdatePixelsData()
    {
        lock (renderingLock)
        {
            _isReverse = _config.IsReverse;
            if (_lineList != null)
            {
                ClearZonePixels();
            }

            _ledRects = new List<Rect>();
            //get all led inside zone
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
                    _ledRects.Add(led.TransformedRect);
                }
            }

            _lineList = GetPixels(_colorApperance);
            if (_isReverse)
                _lineList.Reverse();
            //also update brightness data
            _brightnessData = new float[_lineList.Count];
            UpdateColorsData();
        }
    }

    private void ClearZonePixels()
    {
        lock (_buffer.FrameLock)
        {
            for (int l = 0; l < _lineList.Count; l++)
            {
                var line = _lineList[l];

                for (int i = 0; i < line.Length; i++)
                {
                    ColorComputing.PlotPixel(_buffer, (int)line[i].X, (int)line[i].Y, 0, 0, 0);
                }
            }
        }
    }

    /// <summary>
    /// Update Colors Bank when user change color
    /// </summary>
    private void UpdateColorsData()
    {
        lock (renderingLock)
        {
            _blendMode = _config.Blend;
            _colorBank = ColorComputing
                .GetColorColorBankfromPaletteWithFixedColorPerGap(_config.Colors.ToArray(), _lineList.Count * 8,
                    _blendMode)
                .ToArray();
        }
    }

    /// <summary>
    /// Update Colors Behavior like speed, moving...
    /// </summary>
    private void UpdateColorsBehavior()
    {
        _isColorMoving = _config.IsMoving;
        _colorSpeed = _config.Speed;
        _colorResolution = _config.ColorResolution;
        _isReverse = _config.IsReverse;
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        GC.Collect();
    }

    public CapturingType CaptureType => CapturingType.None;
}