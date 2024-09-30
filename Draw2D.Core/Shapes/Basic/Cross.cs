using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Draw2D.Core.Shapes.Basic
{
    public class Cross : Rectangle
    {
        public Cross(float x, float y, float width, float height) : base(x, y, width, height)
        {
        }

        public Cross(Geo.Rectangle rect) : base(rect)
        {
        }

        public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
        {
            var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
            var thickness = StrokeThickness;
            if (OverrideStrokeStyle)
            {
                strokeBrush = new ImmutableSolidColorBrush(strokeColor);
                thickness = (float)strokeThickness;
            }
            //strokeBrush.Freeze();

            var pen = new ImmutablePen(strokeBrush, thickness);
            {
                DashStyle = DashStyle;
            }
            var immutablePen = pen.ToImmutable();
            ;
            // pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            // fillBrush.Freeze();
            var leftCenter = Canvas.CoordinateSystem.ToScreenSpace(BoundingBox.LeftCenter);
            var rightCenter = Canvas.CoordinateSystem.ToScreenSpace(BoundingBox.RightCenter);
            var bottomCenter = Canvas.CoordinateSystem.ToScreenSpace(BoundingBox.BottomCenter);
            var topCenter = Canvas.CoordinateSystem.ToScreenSpace(BoundingBox.TopCenter);

            dc.DrawLine(immutablePen, new Point(leftCenter[0], leftCenter[1]), new Point(rightCenter[0], rightCenter[1]));
            dc.DrawLine(immutablePen, new Point(bottomCenter[0], bottomCenter[1]), new Point(topCenter[0], topCenter[1]));
        }
    }
}