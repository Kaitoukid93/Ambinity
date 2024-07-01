using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Draw2DControlLibrary
{
    /// <summary>
    /// A class that wraps up zooming and panning of it's content.
    /// </summary>
    public class ZoomAndPanViewBox : ContentControl
    {
        #region local fields

        /// <summary>
        /// The control for creating a drag border
        /// </summary>
        private Border _dragBorder;

        /// <summary>
        /// The control for creating a drag border
        /// </summary>
        private Border _sizingBorder;

        /// <summary>
        /// The control for containing a zoom border
        /// </summary>
        private Canvas _viewportCanvas;

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point _origContentMouseDownPoint;

        #endregion

        #region constructor and overrides

        /// <summary>
        /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
        /// </summary>
        static ZoomAndPanViewBox()
        {
            // DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomAndPanViewBox), new FrameworkPropertyMetadata(typeof(ZoomAndPanViewBox)));
            
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _dragBorder = (this.GetVisualChildren().FirstOrDefault())?.Find<Border>("PART_DraggingBorder");
            _sizingBorder = (this.GetVisualChildren().FirstOrDefault())?.Find<Border>("PART_SizingBorder");
            _viewportCanvas = (this.GetVisualChildren().FirstOrDefault())?.Find<Canvas>("PART_Content");
            SetBackground(Visual);
            if (_dragBorder != null && _viewportCanvas != null)
            {
                _viewportCanvas.PointerPressed += ZoomAndPanControl_MouseDown;
                _viewportCanvas.PointerMoved += ZoomAndPanControl_MouseMove;
                _viewportCanvas.PointerReleased += ZoomAndPanControl_MouseUp;
                DoubleTapped += ZoomAndPanControl_MouseDoubleClick;
            }

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            VisualProperty.Changed.AddClassHandler<ZoomAndPanViewBox>(OnVisualChanged);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs sizeInfo)
        {
            base.OnSizeChanged(sizeInfo);
            if (Bounds.Width > 0)
                _dragBorder.BorderThickness = new Thickness(
                    _viewportCanvas.Bounds.Width / Bounds.Width * BorderThickness.Left,
                    _viewportCanvas.Bounds.Width / Bounds.Width * BorderThickness.Top,
                    _viewportCanvas.Bounds.Width / Bounds.Width * BorderThickness.Right,
                    _viewportCanvas.Bounds.Width / Bounds.Width * BorderThickness.Bottom);
        }

        #endregion

        #region Mouse Event Handlers

        private void ZoomAndPanControl_MouseDown(object sender, PointerEventArgs e)
        {
            GetZoomAndPanControl().SaveZoom();
            _mouseHandlingMode = MouseHandlingMode.Panning;
            _origContentMouseDownPoint = e.GetPosition(_viewportCanvas);

            if (_isShiftDown)
            {
                // Shift + left- or right-down initiates zooming mode.
                _mouseHandlingMode = MouseHandlingMode.DragZooming;
                _dragBorder.IsVisible = false;
                _sizingBorder.IsVisible = true;
                Canvas.SetLeft(_sizingBorder, _origContentMouseDownPoint.X);
                Canvas.SetTop(_sizingBorder, _origContentMouseDownPoint.Y);
                _sizingBorder.Width = 0;
                _sizingBorder.Height = 0;
            }
            else
            {
                // Just a plain old left-down initiates panning mode.
                _mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (_mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                //  _viewportCanvas.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ZoomAndPanControl_MouseUp(object sender, PointerEventArgs e)
        {
            if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                var zoomAndPanControl = GetZoomAndPanControl();
                var curContentPoint = e.GetPosition(_viewportCanvas);
                var rect = Helpers.Clamp(new Point(0, 0), new Point(_viewportCanvas.Width, _viewportCanvas.Height),
                    curContentPoint, _origContentMouseDownPoint);
                zoomAndPanControl.AnimatedZoomTo(rect);
                _dragBorder.IsVisible = true;
                _sizingBorder.IsVisible = false;
            }

            _mouseHandlingMode = MouseHandlingMode.None;
            //  _viewportCanvas.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void ZoomAndPanControl_MouseMove(object sender, PointerEventArgs e)
        {
            if (_mouseHandlingMode == MouseHandlingMode.Panning)
            {
                var curContentPoint = e.GetPosition(_viewportCanvas);
                var rectangleDragVector = curContentPoint - _origContentMouseDownPoint;
                //
                // When in 'dragging rectangles' mode update the position of the rectangle as the user drags it.
                //
                _origContentMouseDownPoint = Helpers.Clamp(e.GetPosition(_viewportCanvas));
                Canvas.SetLeft(_dragBorder, Canvas.GetLeft(_dragBorder) + rectangleDragVector.X);
                Canvas.SetTop(_dragBorder, Canvas.GetTop(_dragBorder) + rectangleDragVector.Y);
            }
            else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                var curContentPoint = e.GetPosition(_viewportCanvas);
                var rect = Helpers.Clamp(new Point(0, 0), new Point(_viewportCanvas.Width, _viewportCanvas.Height),
                    curContentPoint, _origContentMouseDownPoint);
                Helpers.PositionBorderOnCanvas(_sizingBorder, rect);
            }

            e.Handled = true;
        }

        private void ZoomAndPanControl_MouseDoubleClick(object sender, TappedEventArgs e)
        {
            if (_isShiftDown)
            {
                var zoomAndPanControl = GetZoomAndPanControl();
                zoomAndPanControl.SaveZoom();
                zoomAndPanControl.AnimatedSnapTo(e.GetPosition(_viewportCanvas));
            }
        }

        #endregion

        #region Background--Visual Brush

        /// <summary>
        /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
        /// </summary>

        public Control Visual
        {
            get => GetValue(VisualProperty);
            set => SetValue(VisualProperty, value);
        }

        public static readonly StyledProperty<ZoomAndPanViewBox> VisualProperty =
            AvaloniaProperty.Register<Control, ZoomAndPanViewBox>
            (
                "Visual",
                defaultValue: null
            );

        private static void OnVisualChanged(ZoomAndPanViewBox c, AvaloniaPropertyChangedEventArgs e)
        {
            c.SetBackground(e.NewValue as Control);
        }

        private void SetBackground(Control frameworkElement)
        {
            frameworkElement = frameworkElement ?? ((ContentControl)DataContext).Content as Control;
            var visualBrush = new VisualBrush
            {
                Visual = frameworkElement,
                // ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                // ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            if (frameworkElement != null)
                frameworkElement.SizeChanged += (s, e) =>
                {
                    _viewportCanvas.Height = frameworkElement.Bounds.Height;
                    _viewportCanvas.Width = frameworkElement.Bounds.Width;
                    _viewportCanvas.Background = visualBrush;
                };
        }

        #endregion

        private bool _isCtrlDown;

        private bool _isShiftDown;

        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _isCtrlDown = true;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _isShiftDown = true;
        }

        protected void OnKeyUp(object sender, KeyEventArgs e)
        {
            //Canvas?.OnKeyDown(e.Key);
            // if (!e.IsRepeat)
            // {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _isCtrlDown = false;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _isShiftDown = false;
            // }
        }

        private ZoomAndPanControl GetZoomAndPanControl()
        {
            var zoomAndPanControl = this.DataContext as ZoomAndPanControl;
            if (zoomAndPanControl == null)
                throw new NullReferenceException("DataContext is not of type ZoomAndPanControl");
            return zoomAndPanControl;
        }
    }
}