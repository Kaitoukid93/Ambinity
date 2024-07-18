using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Geo;


namespace Draw2D.Core.Shapes.Basic
{
    public class PolyLine : Line
    {
        public PolyLine(Point startPoint, Point endPoint) : base(startPoint, endPoint)
        {
            FillColor = Colors.Transparent;
        }

        public PolyLine(float x1, float y1, float x2, float y2) : base(new Point(x1, x2), new Point(x2, y2))
        {
        }

        public virtual PolyLine AddToEnd(Point point)
        {
            this[PointCount] = point;

            return this;
        }

        public void AddToEnd(float x, float y)
        {
            AddToEnd(new Point(x, y));
        }


        public override IEnumerable<Point> GetSnapPoints()
        {
            if (SnapTargets.HasFlag(SnapTargets.Vertices))
            {
                foreach (var point in Points)
                {
                    yield return point;
                }
            }
        }


        public override bool HitTest(float x, float y)
        {
            for (int i = 0; i < PointCount - 1; i++)
            {
                var startOfLineSegment = this[i];
                var endOfLineSegment = this[i + 1];

                if (Hit(CoronaWidth + StrokeThickness, startOfLineSegment.X,
                        startOfLineSegment.Y, endOfLineSegment.X, endOfLineSegment.Y, x, y))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetPoints(List<Point> points)
        {
            ResetPoints();
            for (int i = 0; i < points.Count; i++)
            {
                this[i] = points[i];
            }


            Canvas?.NeedsRepaint(this);
        }

        public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
        {
            var strokeBrush = new ImmutableSolidColorBrush(Canvas.StrokeColor);
            var thickness = StrokeThickness;
            if (OverrideStrokeStyle)
            {
                strokeBrush = new ImmutableSolidColorBrush(Canvas.StrokeColor);
                thickness = (float)strokeThickness;
            }
            //strokeBrush.Freeze();

            var pen = new ImmutablePen(strokeBrush, thickness);
           // pen.Freeze();

            var geom = new StreamGeometry();
            using (StreamGeometryContext ctx = geom.Open())
            {
                var startVertex = Canvas.CoordinateSystem.ToScreenSpace(StartPoint);
                ctx.BeginFigure(new Avalonia.Point(startVertex[0], startVertex[1]), false);
                int count = 0;
                foreach (var point in Points)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }
                        
                    var vertex = Canvas.CoordinateSystem.ToScreenSpace(point);
                    var v = new Avalonia.Point(vertex[0], vertex[1]);
                    ctx.LineTo(v);
                    count++;
                }
                // ctx.PolyLineTo(Points.Skip(1).Select(p =>
                //     {
                //         var vertex = Canvas.CoordinateSystem.ToScreenSpace(p);
                //         return new Avalonia.Point(vertex[0], vertex[1]);
                //     }).ToList(),
                //     true /* is stroked */, true /* is smooth join */);
            }

            //geom.Freeze();

            dc.DrawGeometry(null, pen, geom);
        }
    }
}