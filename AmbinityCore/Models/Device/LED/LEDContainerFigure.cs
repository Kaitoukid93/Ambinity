
using System.Globalization;
using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace AmbinityCore.Models.Device.LED;

public class LEDContainerFigure : ContainerFigure
{
    private readonly SolidColorBrush _textBrush = new SolidColorBrush(Avalonia.Media.Colors.White);
    public LEDContainerFigure(float x, float y, float width, float height) : base(x, y, width,
        height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        // SnapTargets = SnapTargets.Center | SnapTargets.MidPoints | SnapTargets.Vertices;
    }
    // private bool _isLinked;
    public override void SetChild(IPositionAware child)
    {
        ChildItem = child;
        ItemVisualizer = new LedVisualizer(child);
        ItemVisualizer.ItemUpdated += OnItemUpdate;
        ItemVisualizer.RefreshVisualizer += OnItemVisualizerUpdated;
        Width = (float)ItemVisualizer.Bounds.Width;
        Height = (float)ItemVisualizer.Bounds.Height;
        X = (float)ItemVisualizer.Bounds.X;
        Y = (float)ItemVisualizer.Bounds.Y;
    }
    private void OnItemUpdate()
    {
        Width = (float)ItemVisualizer.Bounds.Width;
        Height = (float)ItemVisualizer.Bounds.Height;
        X = (float)ItemVisualizer.Bounds.X;
        Y = (float)ItemVisualizer.Bounds.Y;
        Canvas.NeedsRepaint(this);
    }

    private void OnItemVisualizerUpdated()
    {
        Canvas.NeedsRepaint(this);
    }
    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
        //get size and location from device property
        // _isLinked = ChildItem.GroupID != Guid.Empty;
        ItemVisualizer.Render(dc, Canvas, IsSelected);
        var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
        var thickness = StrokeThickness;
        if (OverrideStrokeStyle)
        {
            strokeBrush = new ImmutableSolidColorBrush(strokeColor);
            thickness = (float)strokeThickness;
        }
        var _canvasRect = new Rect(0, 0, Canvas.Width, Canvas.Height);
        var rect = new Rect(X, Y, Width, Height);
        bool _isValid = _canvasRect.Contains(rect);
        var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
        var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);
        var fillBrush = new ImmutableSolidColorBrush(FillColor);
        if (!_isValid)
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Red);
        if (IsMouseOver && !IsSelected)
        {
            fillBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray.AdjustOpacity(0.2));
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Orange);
        }

        var pen = new Pen(strokeBrush, thickness, DashStyle);
        var immutablePen = pen.ToImmutable();
        Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
        dc.PushTransform(translate);
        // if (IsSelectable)
        //     dc.DrawRectangle(fillBrush, immutablePen,
        //         new Rect(new Point(X, Y), new Size(Width, Height)));
        if (IsSelected)
            dc.DrawRectangle(fillBrush, immutablePen,
           new Rect(BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height));
        DrawText(dc, (ChildItem as AmbinityLED).Index.ToString());
        // dc.Pop();
        // if (_centerHandle == null && IsSelectable)
        // {
        //     _centerHandle = new ResizeHandle(this, ResizeDirections.Center)
        //     {
        //         Canvas = Canvas,
        //         Position = BoundingBox.Center,


    }
    //should do a text anotation figure to avoid duplication
    private void DrawText(DrawingContext dc, string text)
    {
        var visualWidth = Width * Canvas.ZoomLevel;
        var visualHeight = Height * Canvas.ZoomLevel;
        if (Math.Max(visualWidth, visualHeight) > 20)
        {
            var formattedText = new FormattedText((ChildItem as AmbinityLED).Index.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
           Typeface.Default, 12 / Canvas.ZoomLevel, _textBrush);
            formattedText.SetFontWeight(FontWeight.Bold);
            formattedText.TextAlignment = TextAlignment.Center;
            dc.DrawText(formattedText, new Point(X + Width / 2 - formattedText.Width / 2, Y + Height / 2 - formattedText.Height / 2));
        }


    }


}
