
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Geo;
using Draw2D.Core.Shapes.Basic;
using Point = Draw2D.Core.Geo.Point;

namespace Draw2D.Core.Handles
{
    internal class LineHandle : VectorFigure, IHandle
    {
        private readonly Line _line;
        public VectorFigure HandleShape { get; set; }
        public int LinePointIndex { get; private set; }



        public Figure Owner { get; }

        public LineHandle(VectorFigure handleShape, int linePointIndex, Line line)
        {

            _line = line;
            Owner = line;
            HandleShape = handleShape;
            HandleShape.IsZoomAwareness = true;
            LinePointIndex = linePointIndex;


            Width = HandleShape.Width;
            Height = handleShape.Height;

            Update();

            handleShape.IsDragable = false;
            handleShape.IsVisible = false;
            handleShape.IsSelectable = false;
            handleShape.CanBeSnapTarget = false;

            IsDragable = true;
            IsVisible = true;
            IsSelectable = true;
            CanBeSnapTarget = false;

            SetSnapTargets(SnapTargets.Center);
        }

        public override void OnDrag(Canvas canvas, float dxSum, float dySum, float dx, float dy, bool isShiftKey, bool isCtrlKey)
        {
            if (!IsDragable)
                return;

            var snapDelta = new Point(dx, dy);
            var isSnapped = false;

            foreach (var policy in canvas.GetSnapPolicies())
            {
                Point snapPoint;
                isSnapped = policy.Snap(canvas, HandleShape.Position, dx, dy, dxSum, dySum, out snapPoint, out snapDelta, new[] { this });

                if (isSnapped)
                    break;
            }

            if (isSnapped)
            {
                dx = snapDelta.X;
                dy = snapDelta.Y;
            }

            _line.Translate(dx, dy, LinePointIndex);

            Update();


            Canvas?.NeedsRepaint(this);

        }


        public override void ForceSetPositionCenter(float x, float y)
        {
            base.ForceSetPositionCenter(x, y);
            HandleShape.ForceSetPositionOfCenter(x, y);
        }


        public void Show(Canvas canvas)
        {
            canvas.AddAdornerFigure(this);
            BringToFront();
        }

        public void Hide(Canvas canvas)
        {
            canvas.RemoveAdornerFigure(this);
        }
        public override bool HitTest(float x, float y)
        {
            //get bounding box based on zoom level
            var handleShape = GetCurrentHandleShapeSize();
            if (handleShape == null)
                return false;
            return handleShape.Extented(2 / Canvas.ZoomLevel).HitTest(x, y);
        }
        public void Update()
        {
            ForceSetPositionOfCenter(_line[LinePointIndex].X, _line[LinePointIndex].Y);
            HandleShape.ForceSetPositionOfCenter(_line[LinePointIndex].X, _line[LinePointIndex].Y);
        }

        public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
        {
            if (HandleShape != null)
            {
                var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
                var thickness = StrokeThickness;
                if (OverrideStrokeStyle)
                {
                    strokeBrush = new ImmutableSolidColorBrush(strokeColor);
                    thickness = (float)strokeThickness;
                }
                var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
                var offset = new Avalonia.Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);

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

                var scale = 1 / Canvas.ZoomLevel;
                if (IsMouseOver)
                    fillBrush = new ImmutableSolidColorBrush(Colors.Red);

                dc.DrawEllipse(fillBrush, immutablePen, new Avalonia.Point(BoundingBox.Center.X, BoundingBox.Center.Y), Width * scale / 2, Height * scale / 2);

                // dc.Pop();
            }

        }
        private Draw2D.Core.Geo.Rectangle GetCurrentHandleShapeSize()
        {
            if (Canvas == null || HandleShape == null)
                return null;
            var scale = Canvas.ZoomLevel;
            var newWidth = HandleShape.Width / scale;
            var newHeight = HandleShape.Height / scale;
            var newX = HandleShape.X + (HandleShape.Width - newWidth) / 2;
            var newY = HandleShape.Y + (HandleShape.Height - newHeight) / 2;
            return new Geo.Rectangle(newX, newY, newWidth, newHeight);
        }
    }
}
