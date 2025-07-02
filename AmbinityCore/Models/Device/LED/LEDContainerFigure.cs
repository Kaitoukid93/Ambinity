
using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace AmbinityCore.Models.Device.LED;

public class LEDContainerFigure : ContainerFigure
{
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
        ItemVisualizer.Render(dc, Canvas);
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
        dc.DrawRectangle(fillBrush, immutablePen,
       new Rect(BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height));
        // if (_isLinked && IsSelectable)
        //     dc.DrawRectangle(new SolidColorBrush(Avalonia.Media.Colors.Green.AdjustOpacity(0.2)), null,
        //            new Rect(new Point(X, Y), new Size(Width, Height)));
        // if (IsDragable && IsSelectable)
        // {
        //     // Draw a red lock icon at the top-left corner using the provided geometry string
        //     double iconSize = 16; // Size of the lock icon
        //     var iconX = X + 2;
        //     var iconY = Y + 2;

        //     // Geometry string for the lock icon
        //     string lockGeometryString = "M6.75 3.875C6.75 2.42525 5.5747 1.25 4.125 1.25C3.62669 1.25 3.16082 1.38885 2.76395 1.62997L6.37 5.23605C6.6112 4.83918 6.75 4.37331 6.75 3.875ZM6.9098 6.56952C7.5847 5.87217 8 4.92209 8 3.875C8 1.7349 6.2651 0 4.125 0C3.07791 0 2.12783 0.415307 1.43048 1.09018C1.41415 1.10351 1.39833 1.11778 1.38311 1.13301C1.36789 1.14823 1.35362 1.16404 1.3403 1.18036C0.66535 1.87772 0.25 2.82785 0.25 3.875C0.25 6.0151 1.9849 7.75 4.125 7.75C5.1721 7.75 6.1223 7.33465 6.8196 6.6597C6.836 6.64638 6.8518 6.63211 6.867 6.61689C6.8822 6.60167 6.8965 6.58585 6.9098 6.56952ZM5.4862 6.11996L1.88004 2.51383C1.63888 2.91072 1.5 3.37664 1.5 3.875C1.5 5.32475 2.67525 6.5 4.125 6.5C4.6234 6.5 5.0893 6.36112 5.4862 6.11996Z";

        //     // Parse the geometry
        //     var geometry = Geometry.Parse(lockGeometryString);

        //     // The geometry is designed for an 8x8 box, so scale it to iconSize
        //     var scale = iconSize / 8.0;
        //     var transform = new Matrix(
        //         scale, 0, 0, scale,
        //         iconX, iconY
        //     );
        //     geometry.Transform = new TransformGroup
        //     {
        //         Children = new Transforms()
        //         {
        //             new ScaleTransform(scale, scale),
        //             new TranslateTransform(iconX,iconY)
        //         }
        //     };
        //     dc.DrawGeometry(
        //         new SolidColorBrush(Avalonia.Media.Colors.OrangeRed.AdjustOpacity(0.8)),
        //         null,
        //         geometry
        //     );
        // }
        // if (_isLinked && IsSelectable)
        // {
        //     var linkedBoundingBox = GetLinkedBoundingBox();
        //     if (linkedBoundingBox != null)
        //     {
        //         dc.DrawRectangle(null, immutablePen,
        //             linkedBoundingBox.Value);
        //     }
        // }
        // dc.Pop();
    }


}
