
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Handles;
using Draw2D.Core.Utlils;
using Point = Draw2D.Core.Geo.Point;

namespace Draw2D.Core.Shapes.Basic
{
    public class Ellipse : Rectangle
    {

        public Ellipse(float x, float y, float width, float height) : base(x - width / 2.0f, y - height / 2.0f, width,
            height)
        {
            SetSnapTargets(SnapTargets.MidPoints | SnapTargets.Center);
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
            var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
            var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);

            //strokeBrush.Freeze();

            var pen = new ImmutablePen(strokeBrush, thickness);
            var immutablePen = pen.ToImmutable();
            //  {
            //     DashStyle = DashStyle
            // };
            //pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            //fillBrush.Freeze();
            Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);


            dc.PushTransform(translate);
            var scale = 1.0d;
            if (IsZoomAwareness)
            {
                scale = strokeThickness / 1.5;
                // if(IsSelected)
                // fillBrush = new ImmutableSolidColorBrush(Colors.Red);
            }
            dc.DrawEllipse(fillBrush, immutablePen, new Avalonia.Point(BoundingBox.Center.X, BoundingBox.Center.Y), Width * scale / 2, Height * scale / 2);

            // dc.Pop();
        }
    }
}
