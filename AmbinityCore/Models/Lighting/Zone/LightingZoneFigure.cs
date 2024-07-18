using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneFigure : Rectangle
{
    private FormattedText _text;
    private Point _textOrigin = new Point(5, -30);
    private Pen _defaultPen = new Pen(new SolidColorBrush(Avalonia.Media.Colors.Chartreuse));

    public LightingZoneFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        PositionPropertyChanged += OnPositionChanged;
        SizePropertyChanged += OnSizeChanged;
    }

    private LightingZone _zone;
    public LightingZone Zone => _zone;
    private LightingZoneVisualizer _lightingZoneVisualizer;

    private void OnSizeChanged(float newWidth, float newHeight)
    {
        //update zone
        _lightingZoneVisualizer.UpdateContainerSize(newWidth, newHeight);
    }

    private void OnPositionChanged(float dx, float dy)
    {
        //update zone
        _lightingZoneVisualizer.UpdateContainerOffset(dx, dy);
    }

    public void SetZone(LightingZone zone)
    {
        _lightingZoneVisualizer = new LightingZoneVisualizer(zone);
        _lightingZoneVisualizer.ZoneUpdated += ZoneUpdated;
        _zone = zone;
        _text = new FormattedText(_zone.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default,
            20, new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray));
        _text.MaxTextWidth = 200;
        _text.MaxLineCount = 1;
        _text.Trimming = TextTrimming.CharacterEllipsis;
    }

    /// <summary>
    /// when zone has internal update, notify the container
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ZoneUpdated()
    {
        throw new NotImplementedException();
    }

    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
        //get size and location from device property

        _lightingZoneVisualizer.RenderZone(dc, Canvas);
        var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
        var thickness = StrokeThickness;
        if (OverrideStrokeStyle)
        {
            strokeBrush = new ImmutableSolidColorBrush(strokeColor);
            thickness = (float)strokeThickness;
        }

        var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
        var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);

        // strokeBrush.Freeze();

        //  {
        //  DashStyle = DashStyle
        //  };
        // pen.Freeze();

        var fillBrush = new ImmutableSolidColorBrush(FillColor);
        if (IsMouseOver && !IsSelected)
        {
            fillBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray.AdjustOpacity(0.2));
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Orange);
        }

        var pen = new Pen(strokeBrush, thickness, DashStyle);
        // fillBrush.Freeze();

        Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
        dc.PushTransform(translate);
        dc.DrawRectangle(fillBrush, pen,
            new Rect(new Point(X, Y), new Size(Width, Height)));
        var currentZoomValue = 2 / strokeThickness;
        double adaptiveFontSize = 12d / currentZoomValue;
        _text.SetFontSize(adaptiveFontSize);
        if (IsSelected || IsMouseOver)
            _text.SetForegroundBrush(strokeBrush);
        else
        {
            _text.SetForegroundBrush(new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray));
        }
        if (currentZoomValue is > 0.4 and < 5)
            dc.DrawText(_text, new Point(X, Y - 40));
        // dc.Pop();
    }
}