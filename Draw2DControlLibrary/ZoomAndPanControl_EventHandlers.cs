using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using ZoomAndPan;
using Key = Avalonia.Input.Key;
using MouseButton = Avalonia.Input.MouseButton;

namespace Draw2DControlLibrary
{
    public partial class ZoomAndPanControl
    {
        private void ZoomAndPanControl_EventHandlers_OnApplyTemplate()
        {
            _partDragZoomBorder = (this.GetVisualChildren().FirstOrDefault())?.Find<Border>("PART_DragZoomBorder");
            _partDragZoomCanvas = (this.GetVisualChildren().FirstOrDefault())?.Find<Canvas>("PART_DragZoomCanvas");
            PointerPressed += ZoomAndPanControl_MouseDown;
            PointerReleased += ZoomAndPanControl_MouseUp;
            PointerMoved += ZoomAndPanControl_MouseMove;
            PointerWheelChanged += ZoomAndPanControl_MouseWheel;
            DoubleTapped += ZoomAndPanControl_MouseDoubleClick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            FitCommandDepProp = FitCommand;
            FillCommandDepProp = FillCommand;
            OneHundredPercentCommandDepProp = OneHundredPercentCommand;
            ZoomInCommandDepProp = ZoomInCommand;
            ZoomOutCommandDepProp = ZoomOutCommand;
            UndoZoomCommandDepProp = UndoZoomCommand;
            RedoZoomCommandDepProp = RedoZoomCommand;
        }

        /// <summary>
        /// The control for creating a zoom border
        /// </summary>
        private Border _partDragZoomBorder;

        /// <summary>
        /// The control for containing a zoom border
        /// </summary>
        private Canvas _partDragZoomCanvas;

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point _origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point _origContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton _mouseButtonDown;

        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        ///
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

        private void ZoomAndPanControl_MouseDown(object sender, PointerPressedEventArgs e)
        {
            SaveZoom();
            _content.Focus();
            var point = e.GetCurrentPoint(this);
            var properties = point.Properties;
            if (properties.IsLeftButtonPressed)
            {
                // Left button pressed
                _mouseButtonDown = MouseButton.Left;
            }
            else if (properties.IsRightButtonPressed)
            {
                // Right button pressed
                _mouseButtonDown = MouseButton.Right;
            }

            _origZoomAndPanControlMouseDownPoint = e.GetPosition(this);
            _origContentMouseDownPoint = e.GetPosition(_content);

            if ((_isShiftDown &&
                 (_mouseButtonDown == MouseButton.Left ||
                  _mouseButtonDown == MouseButton.Right)))
            {
                // Shift + left- or right-down initiates zooming mode.
                _mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            else if (_mouseButtonDown == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                _mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (_mouseHandlingMode != MouseHandlingMode.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                //  this.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControl_MouseUp(object sender, PointerReleasedEventArgs e)
        {
            if (_mouseHandlingMode != MouseHandlingMode.None)
            {
                if (_mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (_mouseButtonDown == MouseButton.Left)
                    {
                        // Shift + left-click zooms in on the content.
                        ZoomIn(_origContentMouseDownPoint);
                    }
                    else if (_mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        ZoomOut(_origContentMouseDownPoint);
                    }
                }
                else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
                {
                    var finalContentMousePoint = e.GetPosition(_content);
                    // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                    ApplyDragZoomRect(finalContentMousePoint);
                }

                // this.ReleaseMouseCapture();
                _mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControl_MouseMove(object sender, PointerEventArgs e)
        {
            if (_mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                var curContentMousePoint = e.GetPosition(_content);
                var dragOffset = curContentMousePoint - _origContentMouseDownPoint;

                this.ContentOffsetX -= dragOffset.X;
                this.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (_mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                var curZoomAndPanControlMousePoint = e.GetPosition(this);
                var dragOffset = curZoomAndPanControlMousePoint - _origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (_mouseButtonDown == MouseButton.Left &&
                    (Math.Abs(dragOffset.X) > dragThreshold ||
                     Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    //
                    // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                    // initiate drag zooming mode where the user can drag out a rectangle to select the area
                    // to zoom in on.
                    //
                    _mouseHandlingMode = MouseHandlingMode.DragZooming;
                    var curContentMousePoint = e.GetPosition(_content);
                    InitDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);
                }

                e.Handled = true;
            }
            else if (_mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                //
                // When in drag zooming mode continously update the position of the rectangle
                // that the user is dragging out.
                //
                var curContentMousePoint = e.GetPosition(this);
                SetDragZoomRect(_origZoomAndPanControlMouseDownPoint, curContentMousePoint);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        private void ZoomAndPanControl_MouseWheel(object sender, PointerWheelEventArgs e)
        {
            DelayedSaveZoom750Miliseconds();
            e.Handled = true;

            if (e.Delta.X > 0 || e.Delta.Y > 0)
                ZoomIn(e.GetPosition(_content));
            else if (e.Delta.X < 0 || e.Delta.Y < 0)
                ZoomOut(e.GetPosition(_content));
        }

        /// <summary>
        /// Event raised with the double click command
        /// </summary>
        private void ZoomAndPanControl_MouseDoubleClick(object sender, TappedEventArgs e)
        {
            if (_isShiftDown)
            {
                SaveZoom();
                this.AnimatedSnapTo(e.GetPosition(_content));
            }
        }

        #region private Zoom methods

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter)
        {
            this.ZoomAboutPoint(this.ViewportZoom * 0.90909090909, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter)
        {
            this.ZoomAboutPoint(this.ViewportZoom * 1.1, contentZoomCenter);
        }

        /// <summary>
        /// Initialise the rectangle that the use is dragging out.
        /// </summary>
        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            _partDragZoomCanvas.IsVisible = true;
            _partDragZoomBorder.Opacity = 1;
            SetDragZoomRect(pt1, pt2);
        }

        /// <summary>
        /// Update the position and size of the rectangle that user is dragging out.
        /// </summary>
        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            var rect = Helpers.Clamp(new Point(0, 0),
                new Point(_partDragZoomCanvas.Bounds.Width, _partDragZoomCanvas.Bounds.Height),
                pt1, pt2);
            Helpers.PositionBorderOnCanvas(_partDragZoomBorder, rect);
        }

        /// <summary>
        /// When the user has finished dragging out the rectangle the zoom operation is applied.
        /// </summary>
        private void ApplyDragZoomRect(Point finalContentMousePoint)
        {
            var rect = Helpers.Clamp(new Point(0, 0),
                new Point(_partDragZoomCanvas.Bounds.Width, _partDragZoomCanvas.Bounds.Height),
                finalContentMousePoint, _origContentMouseDownPoint);
            this.AnimatedZoomTo(rect);
            // new Rect(contentX, contentY, contentWidth, contentHeight));
            FadeOutDragZoomRect();
        }

        //
        // Fade out the drag zoom rectangle.
        //
        private void FadeOutDragZoomRect()
        {
            AnimationHelper.StartAnimation(_partDragZoomBorder, OpacityProperty, 0.0, 0.1,
                delegate { _partDragZoomCanvas.IsVisible = false; });
        }

        #endregion

        #region Command DependencyProperties
        
        private ICommand _fitCommandDepProp = default(ICommand);

        public ICommand FitCommandDepProp
        {
            get { return _fitCommandDepProp; }
            set { SetAndRaise(FitCommandDepPropProperty, ref _fitCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> FitCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.FitCommandDepProp,
                (o,v)=>o.FitCommandDepProp = v
                );

        
        private ICommand _fillCommandDepProp = default(ICommand);

        public ICommand FillCommandDepProp
        {
            get { return _fillCommandDepProp; }
            set { SetAndRaise(FillCommandDepPropProperty, ref _fillCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> FillCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.FillCommandDepProp,
                (o,v)=>o.FillCommandDepProp = v
            );

        
        private ICommand _oneHundredPercentCommandDepProp = default(ICommand);

        public ICommand OneHundredPercentCommandDepProp
        {
            get { return _oneHundredPercentCommandDepProp; }
            set { SetAndRaise(OneHundredPercentCommandDepPropProperty, ref _oneHundredPercentCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> OneHundredPercentCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.OneHundredPercentCommandDepProp,
                (o,v)=>o.OneHundredPercentCommandDepProp = v
            );
        
        
        private ICommand _zoomInCommandDepProp = default(ICommand);
        public ICommand ZoomInCommandDepProp
        {
            get { return _zoomInCommandDepProp; }
            set { SetAndRaise(ZoomInCommandDepPropProperty, ref _zoomInCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> ZoomInCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.ZoomInCommandDepProp,
                (o,v)=>o.ZoomInCommandDepProp = v
            );

        
        
        private ICommand _zoomOutCommandDepProp = default(ICommand);
        public ICommand ZoomOutCommandDepProp
        {
            get { return _zoomOutCommandDepProp; }
            set { SetAndRaise(ZoomOutCommandDepPropProperty, ref _zoomOutCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> ZoomOutCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.ZoomOutCommandDepProp,
                (o,v)=>o.ZoomOutCommandDepProp = v
            );


        
        private ICommand _undoZoomCommandDepProp = default(ICommand);
        public ICommand UndoZoomCommandDepProp
        {
            get { return _undoZoomCommandDepProp; }
            set { SetAndRaise(UndoZoomCommandDepPropProperty, ref _undoZoomCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> UndoZoomCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.UndoZoomCommandDepProp,
                (o,v)=>o.UndoZoomCommandDepProp = v
            );

          
        private ICommand _redoZoomCommandDepProp = default(ICommand);
        public ICommand RedoZoomCommandDepProp
        {
            get { return _redoZoomCommandDepProp; }
            set { SetAndRaise(RedoZoomCommandDepPropProperty, ref _redoZoomCommandDepProp, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl,ICommand> RedoZoomCommandDepPropProperty = 
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl,ICommand>(
                "FitCommandDepProp",
                o => o.RedoZoomCommandDepProp,
                (o,v)=>o.RedoZoomCommandDepProp = v
            );
        
        #endregion


        #region Commands

        /// <summary>
        ///     Command to implement the zoom to fill 
        /// </summary>
        public ICommand FillCommand => _fillCommand ?? (_fillCommand = new RelayCommand(() =>
        {
            SaveZoom();
            ZoomTo(ContentFillZoom);
            RaiseCanExecuteChanged();
        }, () => Math.Abs(ViewportZoom - ContentFillZoom) > .01 * ContentFillZoom && ContentFillZoom >= ContentMinZoom));

        private RelayCommand _fillCommand;

        /// <summary>
        ///     Command to implement the zoom to fit 
        /// </summary>
        public ICommand FitCommand => _fitCommand ?? (_fitCommand = new RelayCommand(() =>
        {
            SaveZoom();
            ZoomTo(ContentFitZoom);
            RaiseCanExecuteChanged();
        }, () => Math.Abs(ViewportZoom - ContentFitZoom) > .01 * ContentFitZoom && ContentFitZoom >= ContentMinZoom));

        private RelayCommand _fitCommand;

        /// <summary>
        ///     Command to implement the zoom to 100% 
        /// </summary>
        public ICommand OneHundredPercentCommand => _oneHundredPercentCommand ?? (_oneHundredPercentCommand =
            new RelayCommand(() =>
            {
                SaveZoom();
                AnimatedZoomTo(1.0);
                RaiseCanExecuteChanged();
            }, () => Math.Abs(ViewportZoom - 1.0) > .01 && 1 >= ContentMinZoom));

        private RelayCommand _oneHundredPercentCommand;

        /// <summary>
        ///     Command to implement the zoom out by 110% 
        /// </summary>
        public ICommand ZoomOutCommand => _zoomOutCommand ?? (_zoomOutCommand = new RelayCommand(() =>
        {
            DelayedSaveZoom1500Miliseconds();
            ZoomOut(new Point(ContentZoomFocusX, ContentZoomFocusY));
        }, () => ViewportZoom > MinimumZoom));

        private RelayCommand _zoomOutCommand;

        /// <summary>
        ///     Command to implement the zoom in by 91% 
        /// </summary>
        public ICommand ZoomInCommand => _zoomInCommand ?? (_zoomInCommand = new RelayCommand(() =>
        {
            DelayedSaveZoom1500Miliseconds();
            ZoomIn(new Point(ContentZoomFocusX, ContentZoomFocusY));
        }, () => ViewportZoom < MaximumZoom));

        private RelayCommand _zoomInCommand;

        private void RaiseCanExecuteChanged()
        {
            _oneHundredPercentCommand?.NotifyCanExecuteChanged();
            _zoomOutCommand?.NotifyCanExecuteChanged();
            _zoomInCommand?.NotifyCanExecuteChanged();
            _fitCommand?.NotifyCanExecuteChanged();
            _fillCommand?.NotifyCanExecuteChanged();
        }

        #endregion
    }
}