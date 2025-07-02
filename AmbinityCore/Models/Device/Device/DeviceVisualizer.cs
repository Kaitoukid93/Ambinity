using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Geography;
using AmbinityCore.Visualizer;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Draw2D.Core.Utlils;

namespace Draw2D.Core.Shapes.Basic;

public class DeviceVisualizer : ICanvasVisualizerItem
{
    public event Action ItemUpdated;
    public event Action RefreshVisualizer;

    internal static readonly Dictionary<string, RenderTargetBitmap?> BitmapCache = new();

    //private readonly IRenderService _renderService;
    private readonly List<LedVisualizer> _ledVisualizers;

    private Rect _deviceBounds;
    public Rect Bounds => _deviceBounds;
    private RenderTargetBitmap? _deviceImage;
    private LEDController? _oldDevice;
    private bool _loading;
    private Color[] _previousState = Array.Empty<Color>();
    private AmbinityDevice? _device;
    private readonly ImmutablePen _pen;

    public IPositionAware Item => _device;

    public DeviceVisualizer(IPositionAware device)
    {
        _device = device as AmbinityDevice;
        _device.DeviceUpdate += OnDeviceUpdate;
        _device.ManualLedUpdate += OnDeviceManualLedUpdate;
        _ledVisualizers = new List<LedVisualizer>();
        SetupForDevice();
        var pen = new Pen(new SolidColorBrush(Colors.Black.AdjustOpacity(0.5))) { LineJoin = PenLineJoin.Round };
        _pen = pen.ToImmutable();
    }

    private void OnDeviceManualLedUpdate()
    {
        RefreshVisualizer?.Invoke();
    }

    private void OnDeviceUpdate()
    {
        _deviceBounds = MeasureDevice();
        SetupForDevice();
        ItemUpdated?.Invoke();
    }

    public void UpdateContainerOffset(float dx, float dy)
    {
        _device.X += dx;
        _device.Y += dy;
        // _deviceBounds = MeasureDevice();
    }

    public void UpdateContainerSize(float width, float height)
    {
        double scale = Math.Min(width / _deviceBounds.Width, height / _deviceBounds.Height);
        //  _device.SetScale((float)scale);
    }

    public void Render(DrawingContext dc, Canvas canvas)
    {
        if (_device == null || _deviceBounds.Width == 0 || _deviceBounds.Height == 0 || _loading)
            return;
        // Determine the scale required to fit the desired size of the control
        //double scale = Math.Min(Bounds.Width / _deviceBounds.Width, Bounds.Height / _deviceBounds.Height);
        DrawingContext.PushedState? boundsPush = null;
        try
        {
            // Scale the visualization in the desired bounding box
            //if (Bounds.Width > 0 && Bounds.Height > 0)
            boundsPush = dc.PushTransform(Matrix.CreateScale(_device.Scale, _device.Scale));

            // Apply device rotation
            using DrawingContext.PushedState translationPush =
                dc.PushTransform(Matrix.CreateTranslation(_device.X / _device.Scale, _device.Y / _device.Scale));
            using DrawingContext.PushedState rotationPush =
                dc.PushTransform(Matrix.CreateRotation(Matrix.ToRadians(_device.Rotation)));


            // if (!ShowColors)
            //     return;

            lock (_ledVisualizers)
            {
                // Apply device scale
                // using DrawingContext.PushedState scalePush = dc.PushTransform(Matrix.CreateScale(_device.Scale, _device.Scale));
                // GeometryGroup newGroup = new GeometryGroup();
                foreach (LedVisualizer led in _ledVisualizers)
                {
                    // newGroup.Children.Add(led.DisplayGeometry);
                    led.RenderGeometry(dc);
                }


                // Render device and LED images
                if (_deviceImage != null)
                {
                    if (_device.IsSelectable)
                        dc.DrawImage(_deviceImage, new Rect(_deviceImage.Size),
                            new Rect(0, 0, _device.Layout.ImageWidth, _device.Layout.ImageHeight));
                }



                //
                //dc.DrawGeometry(new ImmutableSolidColorBrush(Colors.Black.AdjustOpacity(0.5)), null, newGroup);
            }
        }
        finally
        {
            boundsPush?.Dispose();
        }
    }

    private async Task SetupForDevice()
    {
        lock (_ledVisualizers)
        {
            _ledVisualizers.Clear();
        }

        // if (_oldDevice != null)
        // {
        //     _oldDevice.DeviceUpdated -= DeviceUpdated;
        // }

        // _oldDevice = Device;
        if (_device == null)
            return;

        _deviceBounds = MeasureDevice();
        _loading = true;

        // Device.DeviceUpdated += DeviceUpdated;

        // Create all the LEDs
        lock (_ledVisualizers)
        {
            foreach (AmbinityLED ambinityLed in _device.Leds)
                _ledVisualizers.Add(new LedVisualizer(ambinityLed));
        }

        // Load the device main image on a background thread
        try
        {
            _deviceImage = await Task.Run(() => GetDeviceImage(_device));
        }
        catch (Exception)
        {
            // ignored
        }

        // InvalidateMeasure();
        _loading = false;
    }

    private Rect MeasureDevice()
    {
        if (_device == null || float.IsNaN(_device.Width) || float.IsNaN(_device.Height))
            return new Rect();

        Rect deviceRect = new(0, 0, _device.Width, _device.Height);
        Geometry geometry = new RectangleGeometry(deviceRect);
        geometry.Transform = new TransformGroup()
        {
            Children =
            {
                new RotateTransform(_device.Rotation),
                new ScaleTransform(_device.Scale, _device.Scale),
                new TranslateTransform(_device.X, _device.Y)
            }
        };


        return geometry.Bounds;
    }

    private RenderTargetBitmap? GetDeviceImage(AmbinityDevice device)
    {
        AmbinityDeviceLayout? layout = device.Layout;
        if (layout == null)
            return null;
        if (string.IsNullOrWhiteSpace(layout.FilePath))
            return null;

        string thumbnailPath;
        try
        {
            thumbnailPath = Path.Combine(layout.FilePath, "thumbnail.png");
        }
        catch (Exception ex)
        {
            // Log or handle invalid path
            return null;
        }

        if (!File.Exists(thumbnailPath))
            return null;

        if (BitmapCache.TryGetValue(layout.FilePath, out RenderTargetBitmap? existingBitmap))
            return existingBitmap;

        if (device.Width <= 0 || device.Height <= 0)
            return null;

        try
        {
            RenderTargetBitmap renderTargetBitmap = layout.RenderLayout((int)device.Width, (int)device.Height);
            BitmapCache[layout.FilePath] = renderTargetBitmap;
            return renderTargetBitmap;
        }
        catch (Exception ex)
        {
            // Log or handle rendering exception
            return null;
        }
    }
}
