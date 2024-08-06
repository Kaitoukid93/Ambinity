using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Draw2D.Core.Shapes.Basic
{
    public class Selectionbox : Rectangle
    {
        public Selectionbox(float x, float y, float width, float height) : base(x, y, width, height)
        {
      
        }

        public Selectionbox(Geo.Rectangle rect) : base(rect)
        {
        }
        public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
        {
            var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
            var thickness = StrokeThickness;
            if (OverrideStrokeStyle)
            {
                strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
                thickness = (float)strokeThickness;
            }
            var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
            var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);
            // strokeBrush.Freeze();
            var pen = new Pen(strokeBrush, thickness, DashStyle);
            //  {
            //  DashStyle = DashStyle
            //  };
            // pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            // fillBrush.Freeze();

            Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
            dc.PushTransform(translate);
            dc.DrawRectangle(fillBrush, pen,
                new Rect(new Point(X, Y), new Size(Width, Height)),1d,1d);

            // dc.Pop();
        }
    }
}
