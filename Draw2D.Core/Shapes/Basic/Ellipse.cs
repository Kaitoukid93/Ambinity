
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
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
            //  {
            //     DashStyle = DashStyle
            // };
            //pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            //fillBrush.Freeze();
            Matrix translate = Matrix.CreateTranslation(offset.X , offset.Y);
            
            dc.PushTransform(translate);

            dc.DrawEllipse(fillBrush, pen, new Avalonia.Point(BoundingBox.Center.X,BoundingBox.Center.Y) , Width / 2, Height / 2);

           // dc.Pop();
        }
    }
}