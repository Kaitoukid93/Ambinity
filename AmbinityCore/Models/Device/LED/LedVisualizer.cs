using System.Globalization;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.Geography;
using AmbinityCore.Visualizer;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Serilog;
using Color = Avalonia.Media.Color;

namespace Draw2D.Core.Shapes.Basic;

public class LedVisualizer : ICanvasVisualizerItem
{
    private readonly SolidColorBrush _fillBrush;
    private readonly ImmutablePen _pen;
    private readonly SolidColorBrush _penBrush;
    private Rect _ledBounds;
    public Rect Bounds => _ledBounds;
    public event Action RefreshVisualizer;
    public event Action ItemUpdated;

    public LedVisualizer(IPositionAware led)
    {
        Led = led as AmbinityLED;
        Led.LEDUpdated += OnLedUpdated;
        _ledBounds = MeasureLED();
        _fillBrush = new SolidColorBrush();
        _penBrush = new SolidColorBrush();
        var pen = new Pen(_penBrush) { LineJoin = PenLineJoin.Round };
        _pen = pen.ToImmutable();
        CreateLedGeometry();
    }

    private void OnLedUpdated()
    {
        _ledBounds = MeasureLED();
        CreateLedGeometry();
        ItemUpdated?.Invoke();
    }

    public AmbinityLED Led { get; }
    public Geometry? DisplayGeometry { get; private set; }


    public IPositionAware Item => Led;

    public void RenderGeometry(DrawingContext drawingContext, bool isSelected = false, bool renderLEDColor = true)
    {
        if (DisplayGeometry == null)
            return;
        if (Led.Device == null)
        {
            if (isSelected)
                Led.LED.SetColor(255, 0, 0);
            else
                Led.LED.SetColor(0, 0, 0);
        }
        if (renderLEDColor)
        {
            _fillBrush.Color = new Color(100, Led.LED.Red, Led.LED.Green, Led.LED.Blue);

            _penBrush.Color = new Color(255, Led.LED.Red, Led.LED.Green, Led.LED.Blue);
        }
        else
        {
            _fillBrush.Color = new Color(100, 0, 0, 0);
            _penBrush.Color = new Color(255, 0, 0, 0);
        }


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
    }

    private void CreateRectangleGeometry()
    {
        DisplayGeometry = new RectangleGeometry(new Rect(Led.X + 0.5, Led.Y + 0.5,
            Led.LedSize.Width - 1, Led.LedSize.Height - 1));
    }

    private void CreateCustomGeometry(double deflateAmount)
    {
        try
        {
            var defAmount = deflateAmount;
            if (Led.Device == null)
                defAmount = 0.5f;
            double width = Led.LedSize.Width - defAmount;
            double height = Led.LedSize.Height - defAmount;
            Geometry geometry;
            geometry = Geometry.Parse(Led.Geometry);
            var boundsLeft = geometry.Bounds.Left;
            var boundsTop = geometry.Bounds.Top;
            var scaleX = width / geometry.Bounds.Width;
            var scaleY = height / geometry.Bounds.Height;
            var left = Led.RelativeX - boundsLeft * scaleX + 0.5 * defAmount;
            var top = Led.RelativeY - boundsTop * scaleY + 0.5 * defAmount;
            if (Led.Device == null)
            {
                left = Led.X - boundsLeft * scaleX + 0.5 * defAmount;
                top = Led.Y - boundsTop * scaleY + 0.5 * defAmount;
            }
            geometry.Transform = new TransformGroup
            {
                Children = new Transforms()
                {
                    new ScaleTransform(scaleX, scaleY),
                    new TranslateTransform(left, top)
                }
            };
            DisplayGeometry = geometry;
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            CreateRectangleGeometry();
        }
    }

    public void UpdateContainerOffset(float dx, float dy)
    {
        Led.X += dx;
        Led.Y += dy;
        _ledBounds = MeasureLED();
        CreateLedGeometry();
        ItemUpdated?.Invoke();
    }

    public void UpdateContainerSize(float width, float height)
    {
        Led.Width = width;
        Led.Height = height;
        _ledBounds = MeasureLED();
        CreateLedGeometry();
        ItemUpdated?.Invoke();
    }

    public void Render(DrawingContext dc, Canvas canvas, bool isSelected = false)
    {
        RenderGeometry(dc, isSelected);
    }
    private Rect MeasureLED()
    {
        if (Led == null || float.IsNaN(Led.Width) || float.IsNaN(Led.Height))
            return new Rect();

        Rect deviceRect = new(0, 0, Led.Width, Led.Height);
        Geometry geometry = new RectangleGeometry(deviceRect);
        geometry.Transform = new TransformGroup()
        {
            Children =
            {
                new RotateTransform(Led.Rotation),
                new ScaleTransform(Led.Scale, Led.Scale),
                new TranslateTransform(Led.X, Led.Y)
            }
        };


        return geometry.Bounds;
    }
}
