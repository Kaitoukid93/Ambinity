using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Media;

namespace Draw2D.Core.Shapes.Basic;

public class LedVisualizer
{
    private readonly SolidColorBrush _fillBrush;
    private readonly Pen _pen;
    private readonly SolidColorBrush _penBrush;

    public LedVisualizer(AmbinityLED led)
    {
        Led = led as AmbinityLED;

        _fillBrush = new SolidColorBrush();
        _penBrush = new SolidColorBrush();
        _pen = new Pen(_penBrush) { LineJoin = PenLineJoin.Round };

        CreateLedGeometry();
    }

    public AmbinityLED Led { get; }
    public Geometry? DisplayGeometry { get; private set; }

    public void RenderGeometry(DrawingContext drawingContext)
    {
        if (DisplayGeometry == null)
            return;

        _fillBrush.Color = new Color(100, Led.LED.Red, Led.LED.Green, Led.LED.Blue);
        _penBrush.Color = new Color(255, Led.LED.Red, Led.LED.Green, Led.LED.Blue);

        // Render the LED geometry
        drawingContext.DrawGeometry(_fillBrush, _pen, DisplayGeometry);
    }

    public bool HitTest(Point position)
    {
        return DisplayGeometry != null && DisplayGeometry.FillContains(position);
    }

    private void CreateLedGeometry()
    {
        // The minimum required size for geometry to be created
        if (Led.LedSize.Width < 2 || Led.LedSize.Height < 2)
            return;
        if (Led.Geometry == null)
            CreateRectangleGeometry();
        else
            CreateCustomGeometry(1.0);

        // switch (Led.RgbLed.Shape)
        // {
        //     case Shape.Custom:
        //         if (Led.RgbLed.Device.DeviceInfo.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
        //             CreateCustomGeometry(2.0);
        //         else
        //             CreateCustomGeometry(1.0);
        //         break;
        //     case Shape.Rectangle:
        //         if (Led.RgbLed.Device.DeviceInfo.DeviceType is RGBDeviceType.Keyboard or RGBDeviceType.Keypad)
        //             CreateKeyCapGeometry();
        //         else
        //             CreateRectangleGeometry();
        //         break;
        //     case Shape.Circle:
        //         CreateCircleGeometry();
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException();
        // }
    }

    private void CreateRectangleGeometry()
    {
        DisplayGeometry = new RectangleGeometry(new Rect(Led.RelativeX + 0.5, Led.RelativeY + 0.5,
            Led.LedSize.Width - 1, Led.LedSize.Height - 1));
    }

    private void CreateCustomGeometry(double deflateAmount)
    {
        try
        {
            double width = Led.LedSize.Width - deflateAmount;
            double height = Led.LedSize.Height - deflateAmount;
            Geometry geometry;
            geometry = Geometry.Parse(Led.Geometry);
            var boundsLeft = geometry.Bounds.Left;
            var boundsTop = geometry.Bounds.Top;
            var scaleX = width / geometry.Bounds.Width;
            var scaleY = height / geometry.Bounds.Height;
            geometry.Transform = new TransformGroup
            {
                Children = new Transforms()
                {
                    new ScaleTransform(scaleX, scaleY),
                    new TranslateTransform(Led.RelativeX - boundsLeft * scaleX, Led.RelativeY - boundsTop * scaleY)
                }
            };
            DisplayGeometry = geometry;
        }
        catch (Exception)
        {
            CreateRectangleGeometry();
        }
    }
}