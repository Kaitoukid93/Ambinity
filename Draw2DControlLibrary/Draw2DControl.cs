using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core;
using Canvas = Avalonia.Controls.Canvas;

namespace Draw2DControlLibrary
{
    public partial class Draw2DControl : Control
    {
        private Pen _gridPen;

        static Draw2DControl()
        {
            /*DefaultStyleKeyProperty.OverrideMetadata(typeof(Draw2DControl),
                new FrameworkPropertyMetadata(typeof(Draw2DControl), FrameworkPropertyMetadataOptions.AffectsRender));*/
        }

        public Draw2DControl()
        {
            _gridPen = new Pen(new ImmutableSolidColorBrush(Colors.Gray), 1);
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
            set { SetValue(CanvasProperty, value); }
        }

        public sealed override void Render(DrawingContext dc)
        {
            if (Canvas == null)
                return;
            DrawBackground(dc);
            // DrawGrid(dc);
            DrawBorder(dc);
           
            // var vectorFigures = Canvas.Figures.OfType<VectorFigure>().Where(f => f.IsVisible).ToList();
            List<VectorFigure> vectorFigures = Canvas.GetRenderableFigures();

            RenderedItemsCount = vectorFigures.Count;

            foreach (var figure in vectorFigures)
            {
                var vectorFigure = figure;

                vectorFigure.Render(dc,_globalBorderThickness,Canvas.StrokeColor);
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

        public void UpdateZoomValue(double value)
        {
            _globalBorderThickness = 2 / value;
            foreach (var figure in Canvas.Figures)
            {
                if (figure is VectorFigure)
                    (figure as VectorFigure).StrokeThickness = (float)_globalBorderThickness;
            }
        }

        private void DrawBorder(DrawingContext dc)
        {  
            IPen pen = new Pen(new ImmutableSolidColorBrush(Canvas.StrokeColor), _globalBorderThickness,DashStyle.Dash);
            var renderSize = Bounds.Size;
            dc.DrawRectangle(pen, new Rect(renderSize));
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