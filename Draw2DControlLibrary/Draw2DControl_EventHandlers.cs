using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Draw2D.Core;
using Draw2D.Core.Policies.RouterPolicy;
using Canvas = Avalonia.Controls.Canvas;


namespace Draw2DControlLibrary
{
    partial class Draw2DControl
    {
        public static readonly StyledProperty<ICanvas> CanvasProperty =
            AvaloniaProperty.Register<Draw2DControl, ICanvas>(
                "Canvas",
                defaultBindingMode: BindingMode.TwoWay,
                defaultValue: default(ICanvas));

        public static readonly StyledProperty<double> WorldMousePosXProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "WorldMousePosX",
                defaultValue: 0.0);

        public double WorldMousePosX
        {
            get => (double)GetValue(WorldMousePosXProperty);
            set => SetValue(WorldMousePosXProperty, value);
        }

        public static readonly StyledProperty<double> WorldMousePosYProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "WorldMousePosY",
                defaultValue: 0.0);

        public double WorldMousePosY
        {
            get => (double)GetValue(WorldMousePosYProperty);
            set => SetValue(WorldMousePosYProperty, value);
        }

        public static readonly StyledProperty<int> RenderedItemsCountProperty =
            AvaloniaProperty.Register<Draw2DControl, int>(
                "RenderedItemsCount",
                defaultValue: 0);

        public int RenderedItemsCount
        {
            get => (int)GetValue(RenderedItemsCountProperty);
            set => SetValue(RenderedItemsCountProperty, value);
        }

        private MouseHandlingMode _mouseHandlingMode = MouseHandlingMode.None;
        private Point _origContentMouseDownPoint;

        private static void OnCanvasChanged(Draw2DControl dependencyObject, AvaloniaPropertyChangedEventArgs e)
        {
            var control = dependencyObject as Draw2DControl;
            if (control == null)
                return;

            if (control.Canvas != null)
            {
                control.Canvas.SceneChanged -= control.CanvasOnSceneChanged;
                control.Canvas.FigureRightClicked -= control.OnFigureRightClicked;
            }

            control.Canvas = e.NewValue as ICanvas;

            if (control.Canvas != null)
            {
                control.Canvas.SceneChanged += control.CanvasOnSceneChanged;
                control.Canvas.FigureRightClicked += control.OnFigureRightClicked;
                control.Width = control.Canvas.Width;
                control.Height = control.Canvas.Height;
            }

            control.InvalidateVisual();
        }

        public static readonly StyledProperty<double> ViewportWidthProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "ViewportWidth",
                defaultValue: default(double)
            );


        private static void ViewportWidthChanged(Draw2DControl dependencyObject,
            AvaloniaPropertyChangedEventArgs e)
        {
            var control = dependencyObject as Draw2DControl;

            if (control?.Canvas != null)
            {
                control.Canvas.ViewportWidth = (double)e.NewValue;
            }
        }

        public double ViewportWidth
        {
            get => GetValue(ViewportWidthProperty);
            set => SetValue(ViewportWidthProperty, value);
        }

        public static readonly StyledProperty<double> ViewportHeightProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "ViewportHeight",
                defaultValue: default(double)
            );

        private static void ViewportHeightChanged(Draw2DControl dependencyObject,
            AvaloniaPropertyChangedEventArgs e)
        {
            var control = dependencyObject as Draw2DControl;

            if (control?.Canvas != null)
            {
                control.Canvas.ViewportHeight = (double)e.NewValue;
            }
        }

        public double ViewportHeight
        {
            get => GetValue(ViewportHeightProperty);
            set => SetValue(ViewportHeightProperty, value);
        }

        public static readonly StyledProperty<double> ContentOffsetXProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "ContentOffsetX",
                defaultValue: default(double)
            );

        private static void ContentOffsetXChanged(Draw2DControl dependencyObject,
            AvaloniaPropertyChangedEventArgs e)
        {
            var control = dependencyObject as Draw2DControl;

            if (control?.Canvas != null)
            {
                control.Canvas.ContentOffsetX = (double)e.NewValue;
            }
        }

        public double ContentOffsetX
        {
            get => GetValue(ContentOffsetXProperty);
            set => SetValue(ContentOffsetXProperty, value);
        }

        public static readonly StyledProperty<double> ContentOffsetYProperty =
            AvaloniaProperty.Register<Draw2DControl, double>(
                "ContentOffsetY",
                default(double));

        private static void ContentOffsetYChanged(Draw2DControl dependencyObject,
            AvaloniaPropertyChangedEventArgs e)
        {
            var control = dependencyObject as Draw2DControl;

            if (control?.Canvas != null)
            {
                control.Canvas.ContentOffsetY = (double)e.NewValue;
            }
        }


        public double ContentOffsetY
        {
            get => GetValue(ContentOffsetYProperty);
            set => SetValue(ContentOffsetYProperty, value);
        }


        private void OnFigureRightClicked(object sender, FigureClickEventArgs e)
        {
          // Dispatcher.UIThread.Invoke(() => ShowFlyOut(e.Sender, e.MousePosX, e.MousePosY));
        }


      

        private void CanvasOnSceneChanged(object sender, EventArgs eventArgs)
        {
            //Debug.WriteLine("CanvasOnSceneChanged");
            Dispatcher.UIThread.Post(InvalidateVisual);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            // this.CapturePointer();
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed)
            {
                if (e.ClickCount > 1)
                {
                    Canvas?.OnMouseLeftDoubleClick(point.Position.X, point.Position.Y, IsShifKeyDown(),
                        IsControlKeyDown());
                }
                else
                {
                    Canvas?.OnMouseLeftDown(point.Position.X, point.Position.Y, IsShifKeyDown(), IsControlKeyDown());
                }
            }

            if (point.Properties.IsRightButtonPressed)
            {
                Canvas?.OnMouseRightDown(point.Position.X, point.Position.Y, IsShifKeyDown(), IsControlKeyDown());
            }

            if (point.Properties.IsMiddleButtonPressed == true)
            {
                _mouseHandlingMode = MouseHandlingMode.Panning;
                _origContentMouseDownPoint = e.GetPosition(this);
            }

            e.Handled = true;
        }


        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            _mouseHandlingMode = MouseHandlingMode.None;
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Canvas?.OnMouseLeftUp(point.Position.X, point.Position.Y, IsShifKeyDown(), IsControlKeyDown());
            }
            else if (e.InitialPressMouseButton == MouseButton.Right)
            {
                Canvas?.OnMouseRightUp(point.Position.X, point.Position.Y, IsShifKeyDown(), IsControlKeyDown());
            }

            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_mouseHandlingMode == MouseHandlingMode.Panning)
            {
                //
                // The user is left-dragging the mouse.
                // Pan the viewport by the appropriate amount.
                //
                var curContentMousePoint = e.GetPosition(this);
                var dragOffset = curContentMousePoint - _origContentMouseDownPoint;

                this.ContentOffsetX -= dragOffset.X;
                this.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else
            {
                var screenPoint = e.GetPosition(this);
                //Check if we can be informed from the canvas about world mouse pos.
                var point = Canvas.CoordinateSystem.ToWorldSpace(screenPoint.X, screenPoint.Y);

                WorldMousePosX = point.X;
                WorldMousePosY = point.Y;

                Canvas?.OnMouseMove(screenPoint.X, screenPoint.Y, IsShifKeyDown(), IsControlKeyDown());

                e.Handled = true;
            }
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            var hasFocused = this.Focus();
             
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            //ClearFocus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //Canvas?.OnKeyDown(e.Key);
            // if (!e.IsRepeat)
            // {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _isCtrlDown = true;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _isShiftDown = true;
            Canvas?.OnKeyDown(e.Key);
            // }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            //Canvas?.OnKeyDown(e.Key);
            // if (!e.IsRepeat)
            // {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                _isCtrlDown = false;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _isShiftDown = false;
            Canvas?.OnKeyUp(e.Key);
            // }
        }

        private static bool _isCtrlDown;

        private static bool IsControlKeyDown()
        {
            return _isCtrlDown;
        }

        private static bool _isShiftDown;

        private static bool IsShifKeyDown()
        {
            return _isShiftDown;
        }
    }
}