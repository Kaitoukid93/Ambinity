using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;


namespace Draw2D.Core.Shapes.Basic
{
    public class CustomGeometryShape : Rectangle
    {
        // private VectorFigure _centerHandle;
        private readonly string _geometry;
        private Geometry? _displayGeometry;
        public CustomGeometryShape(string geometry, float x, float y, float width, float height) : base(x, y, width,
            height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            SnapTargets = SnapTargets.Center | SnapTargets.MidPoints | SnapTargets.Vertices;
            _geometry = geometry;
            PositionPropertyChanged += UpdateGeometry;
           // SizePropertyChanged += UpdateGeometry;
            CreateCustomGeometry();
        }

        private void UpdateGeometry(float dx, float dy)
        {
            CreateCustomGeometry();
        }

        /// <summary>
        /// padding this rectangle respect specific amount
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public void Padding(float left, float top, float right, float bottom)
        {
            this.X += left;
            this.Y += top;
            this.Width -= left + right;
            this.Height -= top + bottom;
        }

        public override bool OnDragStart(Canvas canvas, float x, float y, Figure mouseDownElement = null)
        {
            base.OnDragStart(canvas, x, y, mouseDownElement);

            // if (_centerHandle == null)
            // {
            //     _centerHandle = new Cross(BoundingBox.Center.X, BoundingBox.Center.Y, 10, 10)
            //     {
            //         IsSelectable = false,
            //         IsDragable = false,
            //         FillColor = Colors.Transparent,
            //         CanBeSnapTarget = false
            //     };
            //     // canvas.AddFigure(_centerHandle);
            // }

            // _centerHandle.BringToFront();


            return true;
        }

        public override void OnDrag(Canvas canvas, float dxSum, float dySum, float dx, float dy, bool isShiftKey,
            bool isCtrlKey)
        {
            base.OnDrag(canvas, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey);

            //  _centerHandle?.ForceSetPositionOfCenter(BoundingBox.Center);
        }

        public override void OnDragEnd(Canvas canvas, bool isShiftKey, bool isCtrlKey)
        {
            base.OnDragEnd(canvas, isShiftKey, isCtrlKey);
            // canvas.RemoveFigure(_centerHandle);
            // _centerHandle = null;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, w: {Width}, h: {Height}";
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
            var immutablePen = pen.ToImmutable();
            //  {
            //  DashStyle = DashStyle
            //  };
            // pen.Freeze();

            var fillBrush = new ImmutableSolidColorBrush(FillColor);
            // fillBrush.Freeze();

            Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
            dc.PushTransform(translate);
            if (_displayGeometry != null)
                dc.DrawGeometry(fillBrush, immutablePen, _displayGeometry);

            // dc.Pop();
        }
        private void CreateCustomGeometry()
        {
            try
            {

                Geometry geometry;
                geometry = Geometry.Parse(_geometry);
                var boundsLeft = geometry.Bounds.Left;
                var boundsTop = geometry.Bounds.Top;
                var scaleX = Width / geometry.Bounds.Width;
                var scaleY = Height / geometry.Bounds.Height;
                var left = X - boundsLeft * scaleX;
                var top = Y - boundsTop * scaleY;

                geometry.Transform = new TransformGroup
                {
                    Children = new Transforms()
                {
                    new ScaleTransform(scaleX, scaleY),
                    new TranslateTransform(left, top)
                }
                };
                _displayGeometry = geometry;
            }
            catch (Exception ex)
            {

                CreateRectangleGeometry();
            }
        }
        private void CreateRectangleGeometry()
        {
            _displayGeometry = new RectangleGeometry(new Rect(X, Y,
               Width, Height));
        }
    }
}
