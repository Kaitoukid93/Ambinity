using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Draw2D.Core;
using Draw2D.Core.Utlils;
using Canvas = Avalonia.Controls.Canvas;

namespace Draw2DControlLibrary
{
    public partial class Draw2DControl : Control
    {
        private Pen _gridPen;
        private Pen _borderPen;
        private object frameLock = new object();
        private WriteableBitmap _reusableBitmap;

        static Draw2DControl()
        {
            /*DefaultStyleKeyProperty.OverrideMetadata(typeof(Draw2DControl),
                new FrameworkPropertyMetadata(typeof(Draw2DControl), FrameworkPropertyMetadataOptions.AffectsRender));*/
        }

        public Draw2DControl()
        {
            _gridPen = new Pen(new ImmutableSolidColorBrush(Colors.Gray.AdjustOpacity(0.2)), 1);
            _borderPen = new Pen(new ImmutableSolidColorBrush(Colors.Gray), 1);
            CanvasProperty.Changed.AddClassHandler<Draw2DControl>(OnCanvasChanged);
            //  _gridPen.Freeze();
            //Background = Brushes.Transparent;
            RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);
            ContentOffsetYProperty.Changed.AddClassHandler<Draw2DControl>(ContentOffsetYChanged);
            ContentOffsetXProperty.Changed.AddClassHandler<Draw2DControl>(ContentOffsetXChanged);
            ViewportWidthProperty.Changed.AddClassHandler<Draw2DControl>(ViewportWidthChanged);
            ViewportHeightProperty.Changed.AddClassHandler<Draw2DControl>(ViewportHeightChanged);
        }


        public ICanvas Canvas
        {
            get { return (ICanvas)GetValue(CanvasProperty); }
            set
            {
                SetValue(CanvasProperty, value);
                UpdateBitmap();
            }
        }

        private void UpdateBitmap()
        {
            Vector dpi = new Vector(96, 96);
            _reusableBitmap = new WriteableBitmap(
                new PixelSize((int)Canvas.Width, (int)Canvas.Height),
                dpi,
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);
        }

        public sealed override void Render(DrawingContext dc)
        {
            if (Canvas == null)
                return;
            DrawBackground(dc);
            // DrawGrid(dc);
            DrawBorder(dc);
            if (_zoomValue > 5)
                DrawGrid(dc);
            //var vectorFigures = Canvas.Figures.OfType<VectorFigure>().Where(f => f.IsVisible).ToList();

            if (Canvas.BackgroundImageBuffer != null && Canvas.ShouldDrawBackgroundImage)
            {
                using (var frameBuffer = _reusableBitmap.Lock())
                {
                    lock (Canvas.BackgroundImageBuffer.FrameLock)
                    {
                        Marshal.Copy(Canvas.BackgroundImageBuffer.PixelData, 0, frameBuffer.Address,
                            Canvas.BackgroundImageBuffer.PixelData.Length);
                    }

                    dc.DrawImage(_reusableBitmap, new Rect(0, 0, Canvas.Width, Canvas.Height));
                }
            }

            List<VectorFigure> vectorFigures = Canvas.GetRenderableFigures();

            RenderedItemsCount = vectorFigures.Count;

            foreach (var figure in vectorFigures)
            {
                var vectorFigure = figure;

                vectorFigure.Render(dc, _globalBorderThickness, Canvas.StrokeColor);
            }

            base.Render(dc);
        }

        private void DrawBackground(DrawingContext dc)
        {
            IBrush background = new SolidColorBrush() { Color = Colors.Transparent };
            var renderSize = Bounds.Size;
            dc.FillRectangle(background, new Rect(renderSize));
        }

        private double _globalBorderThickness = 1;
        private double _zoomValue = 1;

        public void UpdateZoomValue(double value)
        {
            _zoomValue = value;
            _globalBorderThickness = 1.5d / value;
            foreach (var figure in Canvas.Figures)
            {
                if (figure is VectorFigure)
                    (figure as VectorFigure).StrokeThickness = (float)_globalBorderThickness;
            }

            _gridPen.Thickness = 1 / _zoomValue;
        }

        private void DrawBorder(DrawingContext dc)
        {
            _borderPen.Brush = new ImmutableSolidColorBrush(Canvas.StrokeColor);
            _borderPen.DashStyle = new ImmutableDashStyle(new double[] { 7, 3 }, 1);
            _borderPen.Thickness = _globalBorderThickness;
            var renderSize = Bounds.Size;
            dc.DrawRectangle(_borderPen, new Rect(renderSize));
        }

        private void DrawGrid(DrawingContext dc)
        {
            var xGridSize = Canvas.Grid.UnitX;
            var xAmount = (int)Canvas.Width / xGridSize;

            var yGridSize = Canvas.Grid.UnitY;
            var yAmount = (int)Canvas.Height / yGridSize;

            for (int i = 0; i <= xAmount; i++)
            {
                dc.DrawLine(_gridPen, new Point(i * xGridSize, 0), new Point(i * xGridSize, Canvas.Height));
            }

            for (int i = 0; i <= yAmount; i++)
            {
                dc.DrawLine(_gridPen, new Point(0, i * yGridSize), new Point(Canvas.Width, i * yGridSize));
            }
        }


        //private Draw2D.Core.Geo.Point ToWorldSpace(Point screenPoint)
        //{
        //    var factor = 1; //internally everthing is stored in 1/100mm.
        //    return new Draw2D.Core.Geo.Point((int) (screenPoint.X - Canvas.OriginX*Canvas.Width*factor),
        //        (int) (Canvas.Height*factor - screenPoint.Y - Canvas.OriginY *Canvas.Height*factor));
        //}

        //private Point ToScreenSpace(Draw2D.Core.Geo.Point point)
        //{
        //    var factor = 1; //internally everthing is stored in 1/100mm.
        //    return new Point(point.X + Canvas.OriginX * Canvas.Width / factor,
        //        -point.Y + Canvas.Height / factor - Canvas.OriginY * Canvas.Height / factor);
        //}
    }
}