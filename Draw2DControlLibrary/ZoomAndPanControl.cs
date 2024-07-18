using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Draw2DControlLibrary
{
    /// <summary>
    /// A class that wraps up zooming and panning of it's content.
    /// </summary>
    public partial class ZoomAndPanControl : ContentControl, IScrollable, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// this is the brush size that never change 
        /// </summary>
        private double _constantSizeBrush = 2.0;

        /// <summary>
        /// Reference to the underlying content, which is named PART_Content in the template.
        /// </summary>
        private Control _content = null;

        /// <summary>
        /// The transform that is applied to the content to scale it by 'ViewportZoom'.
        /// </summary>
        private ScaleTransform _contentZoomTransform = null;

        /// <summary>
        /// The transform that is applied to the content to offset it by 'ContentOffsetX' and 'ContentOffsetY'.
        /// </summary>
        private TranslateTransform _contentOffsetTransform = null;

        /// <summary>
        /// The height of the viewport in content coordinates, clamped to the height of the content.
        /// </summary>
        private double _constrainedContentViewportHeight = 0.0;

        /// <summary>
        /// The width of the viewport in content coordinates, clamped to the width of the content.
        /// </summary>
        private double _constrainedContentViewportWidth = 0.0;

        /// <summary>
        /// Normally when content offsets changes the content focus is automatically updated.
        /// This syncronization is disabled when 'disableContentFocusSync' is set to 'true'.
        /// When we are zooming in or out we 'disableContentFocusSync' is set to 'true' because 
        /// we are zooming in or out relative to the content focus we don't want to update the focus.
        /// </summary>
        private bool _disableContentFocusSync = false;

        /// <summary>
        /// Enable the update of the content offset as the content scale changes.
        /// This enabled for zooming about a point (google-maps style zooming) and zooming to a rect.
        /// </summary>
        private bool _enableContentOffsetUpdateFromScale = false;

        /// <summary>
        /// Used to disable syncronization between IScrollInfo interface and ContentOffsetX/ContentOffsetY.
        /// </summary>
        private bool _disableScrollOffsetSync = false;

        #endregion

        #region constructor and overrides

        /// <summary>
        /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
        /// </summary>
        static ZoomAndPanControl()
        {
            // ThemeProperty.OverrideMetadata(typeof(ZoomAndPanControl), new StyledPropertyMetadata<ControlTheme?>(typeof(ZoomAndPanControl)));
        }

        public ZoomAndPanControl()
        {
            ViewportZoomProperty.Changed.AddClassHandler<ZoomAndPanControl>(ViewportZoom_PropertyChanged);
            ContentOffsetXProperty.Changed.AddClassHandler<ZoomAndPanControl>(ContentOffsetX_PropertyChanged);
            ContentOffsetYProperty.Changed.AddClassHandler<ZoomAndPanControl>(ContentOffsetY_PropertyChanged);
            MinimumZoomProperty.Changed.AddClassHandler<ZoomAndPanControl>(MinimumOrMaximumZoom_PropertyChanged);
            MaximumZoomProperty.Changed.AddClassHandler<ZoomAndPanControl>(MinimumOrMaximumZoom_PropertyChanged);
            this.SizeChanged += OnRenderSizeChanged;

            // CommandBindings.Add(new CommandBinding(MyFillCommand, OnMyFillCommandExecuted, CanExecuteFillCommand));
        }

        /*private void OnMyFillCommandExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            var zoomPanControl = (ZoomAndPanControl)sender;
            zoomPanControl.SaveZoom();
            zoomPanControl.ZoomTo(zoomPanControl.ContentFillZoom);
            zoomPanControl.RaiseCanExecuteChanged();
        }

        private static void CanExecuteFillCommand(object sender, e)
        {
            var zoomPanControl = (ZoomAndPanControl)sender;
            e.CanExecute =
                Math.Abs(zoomPanControl.ViewportZoom - zoomPanControl.ContentFillZoom) >
                .01 * zoomPanControl.ContentFillZoom && zoomPanControl.ContentFillZoom >= zoomPanControl.ContentMinZoom;

            e.Handled = true;
        }*/


        /// <summary>
        /// Need to update zoom values if size changes, and update ViewportZoom if too low
        /// </summary>
        protected void OnRenderSizeChanged(object sender, SizeChangedEventArgs sizeInfo)
        {
            //  base.OnSizeChanged(sizeInfo);
            if (ViewportZoom < ContentMinZoom) ViewportZoom = ContentMinZoom;
            //
            // INotifyPropertyChanged property update
            //
            OnPropertyChanged(nameof(ContentMinZoom));
            OnPropertyChanged(nameof(ContentFillZoom));
            OnPropertyChanged(nameof(ContentFitZoom));
        }

        /// <summary>
        /// Called when a template has been applied to the control.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            // _content = this.GetControl<Control>("123");

            _content = (this.GetVisualChildren().FirstOrDefault() as ContentPresenter).Content as Draw2DControl;

            if (_content != null)
            {
                _content.RenderTransformOrigin = RelativePoint.TopLeft;
                //
                // Setup the transform on the content so that we can scale it by 'ViewportZoom'.
                //
                this._contentZoomTransform = new ScaleTransform(this.ViewportZoom, this.ViewportZoom);

                //
                // Setup the transform on the content so that we can translate it by 'ContentOffsetX' and 'ContentOffsetY'.
                //
                this._contentOffsetTransform = new TranslateTransform();
                UpdateTranslationX();
                UpdateTranslationY();

                //
                // Setup a transform group to contain the translation and scale transforms, and then
                // assign this to the content's 'RenderTransform'.
                //
                var transformGroup = new TransformGroup();
                transformGroup.Changed += transformChanged;
                transformGroup.Children.Add(this._contentOffsetTransform);
                transformGroup.Children.Add(this._contentZoomTransform);
                _content.RenderTransform = transformGroup;
                (_content as Draw2DControl).UpdateZoomValue(ViewportZoom);
                ZoomAndPanControl_EventHandlers_OnApplyTemplate();
            }
        }

        private void transformChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Measure the control and it's children.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            var infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            var childSize = base.MeasureOverride(infiniteSize);

            if (childSize != _unScaledExtent)
            {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                _unScaledExtent = childSize;
                ScrollOwner?.InvalidateVisual();
            }

            //
            // Update the size of the viewport onto the content based on the passed in 'constraint'.
            //
            UpdateViewportSize(constraint);

            var width = constraint.Width;
            var height = constraint.Height;

            if (double.IsInfinity(width))
                width = childSize.Width;

            if (double.IsInfinity(height))
                height = childSize.Height;

            UpdateTranslationX();
            UpdateTranslationY();

            return new Size(width, height);
        }

        /// <summary>
        /// Arrange the control and it's children.
        /// </summary>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(this.DesiredSize);

            if (_content.DesiredSize != _unScaledExtent)
            {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                _unScaledExtent = _content.DesiredSize;
                ScrollOwner?.InvalidateVisual();
            }

            //
            // Update the size of the viewport onto the content based on the passed in 'arrangeBounds'.
            //
            UpdateViewportSize(arrangeBounds);

            return size;
        }

        #endregion

        #region IScrollInfo Data Members

        //
        // These data members are for the implementation of the IScrollInfo interface.
        // This interface works with the ScrollViewer such that when ZoomAndPanControl is 
        // wrapped (in XAML) with a ScrollViewer the IScrollInfo interface allows the ZoomAndPanControl to
        // handle the the scrollbar offsets.
        //
        // The IScrollInfo properties and member functions are implemented in ZoomAndPanControl_IScrollInfo.cs.
        //
        // There is a good series of articles showing how to implement IScrollInfo starting here:
        //     http://blogs.msdn.com/bencon/archive/2006/01/05/509991.aspx
        //

        /// <summary>
        /// Records the unscaled extent of the content.
        /// This is calculated during the measure and arrange.
        /// </summary>
        private Size _unScaledExtent = new Size(0, 0);

        /// <summary>
        /// Records the size of the viewport (in viewport coordinates) onto the content.
        /// This is calculated during the measure and arrange.
        /// </summary>
        private Size _viewport = new Size(0, 0);

        #endregion IScrollInfo Data Members

        #region Dependency Property Definitions

        //
        // Definitions for dependency properties.
        //

        /// <summary>
        /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
        /// </summary>
        private double _animationDuration = 0.4;

        public double AnimationDuration
        {
            get { return _animationDuration; }
            set { SetAndRaise(AnimationDurationProperty, ref _animationDuration, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, double> AnimationDurationProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, double>("AnimationDuration",
                o => o.AnimationDuration
                , (o, v) => o.AnimationDuration = v
            );

        /// <summary>
        /// Get/set the X offset (in content coordinates) of the view on the content.
        /// </summary>

        public double ContentOffsetX
        {
            get { return (double)GetValue(ContentOffsetXProperty); }
            set { SetValue(ContentOffsetXProperty, value); }
        }

        public static readonly StyledProperty<double> ContentOffsetXProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>("ContentOffsetX",
                defaultValue: 0.0,
                coerce: ContentOffsetX_Coerce
            );

        /// <summary>
        /// Get/set the Y offset (in content coordinates) of the view on the content.
        /// </summary>

        public double ContentOffsetY
        {
            get { return (double)GetValue(ContentOffsetYProperty); }
            set { SetValue(ContentOffsetYProperty, value); }
        }

        public static readonly StyledProperty<double> ContentOffsetYProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>("ContentOffsetY",
                defaultValue: 0.0,
                coerce: ContentOffsetY_Coerce
            );

        /// <summary>
        /// Get/set the current scale (or zoom factor) of the content.
        /// </summary>
        public double ViewportZoom
        {
            get { return (double)GetValue(ViewportZoomProperty); }
            set { SetValue(ViewportZoomProperty, value); }
        }

        public static readonly StyledProperty<double> ViewportZoomProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>("ViewportZoom",
                defaultValue: 1.0,
                coerce: ViewportZoom_Coerce);

        /// <summary>
        /// Get the viewport height, in content coordinates.
        /// </summary>
        private double _contentViewPortHeight = 0.0;

        public double ContentViewportHeight
        {
            get { return _contentViewPortHeight; }
            set { SetAndRaise(ContentViewportHeightProperty, ref _contentViewPortHeight, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, double> ContentViewportHeightProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, double>
            (
                "ContentViewportHeight",
                o => o.ContentViewportHeight,
                (o, v) => o.ContentViewportHeight = v
            );

        /// <summary>
        /// Get the viewport width, in content coordinates.
        /// </summary>
        private double _contentViewportWidth = 0.0;

        public double ContentViewportWidth
        {
            get { return _contentViewportWidth; }
            set { SetAndRaise(ContentViewportWidthProperty, ref _contentViewportWidth, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, double> ContentViewportWidthProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, double>
            (
                "ContentViewPortWidth",
                o => o.ContentViewportWidth,
                (o, v) => o.ContentViewportWidth = v
            );

        /// <summary>
        /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
        /// </summary>


        public double ContentZoomFocusX
        {
            get => GetValue(ContentZoomFocusXProperty);
            set => SetValue(ContentZoomFocusXProperty, value);
        }

        public static readonly StyledProperty<double> ContentZoomFocusXProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>
            (
                "ViewportZoomFocusX",
                defaultValue: 0.0
            );

        /// <summary>
        /// The Y coordinate of the content focus, this is the point that we are focusing on when zooming.
        /// </summary>
        public double ContentZoomFocusY
        {
            get => GetValue(ContentZoomFocusYProperty);
            set => SetValue(ContentZoomFocusYProperty, value);
        }

        public static readonly StyledProperty<double> ContentZoomFocusYProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>
            (
                "ViewportZoomFocusY",
                defaultValue: 0.0
            );


        /// <summary>
        /// Set to 'true' to enable the mouse wheel to scroll the zoom and pan control.
        /// This is set to 'false' by default.
        /// </summary>
        private bool _isMouseWheelScrollingEnabled = false;

        public bool IsMouseWheelScrollingEnabled
        {
            get { return _isMouseWheelScrollingEnabled; }
            set { SetAndRaise(IsMouseWheelScrollingEnabledProperty, ref _isMouseWheelScrollingEnabled, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, bool> IsMouseWheelScrollingEnabledProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, bool>
            (
                "IsMouseWheelScrollingEnabled",
                o => o.IsMouseWheelScrollingEnabled,
                (o, v) => o.IsMouseWheelScrollingEnabled = v
            );


        /// <summary>
        /// Get/set the maximum value for 'ViewportZoom'.
        /// </summary>
        private double _maximumZoom = 10.0;

        public double MaximumZoom
        {
            get { return _maximumZoom; }
            set { SetAndRaise(MaximumZoomProperty, ref _maximumZoom, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, double> MaximumZoomProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, double>
            (
                "MaximumZoom",
                o => o.MaximumZoom,
                (o, v) => o.MaximumZoom = v
            );

        /// <summary>
        /// Get/set the maximum value for 'ViewportZoom'.
        /// </summary>
        private MinimumZoomType _minimumZoomType = MinimumZoomType.MinimumZoom;

        public MinimumZoomType MinimumZoomType
        {
            get { return _minimumZoomType; }
            set { SetAndRaise(MinimumZoomTypeProperty, ref _minimumZoomType, value); }
        }

        public static readonly DirectProperty<ZoomAndPanControl, MinimumZoomType> MinimumZoomTypeProperty =
            AvaloniaProperty.RegisterDirect<ZoomAndPanControl, MinimumZoomType>
            (
                "MinimumZoomType",
                o => o.MinimumZoomType,
                (o, v) => o.MinimumZoomType = v
            );

        /// <summary>
        /// Get/set the MinimumZoom value for 'ViewportZoom'.
        /// </summary>;

        public double MinimumZoom
        {
            get => GetValue(MinimumZoomProperty);
            set => SetValue(MinimumZoomProperty, value);
        }

        public static readonly StyledProperty<double> MinimumZoomProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>
            (
                "MinimumZoom",
                defaultValue: 0.0
            );

        /// <summary>
        /// The X coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
        /// that the content focus point is locked to while zooming in.
        /// </summary>
        public double ViewportZoomFocusX
        {
            get => GetValue(ViewportZoomFocusXProperty);
            set => SetValue(ViewportZoomFocusXProperty, value);
        }

        public static readonly StyledProperty<double> ViewportZoomFocusXProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>
            (
                "ViewportZoomFocusX",
                defaultValue: 0.0
            );

        /// <summary>
        /// The Y coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
        /// that the content focus point is locked to while zooming in.
        /// </summary>
        public double ViewportZoomFocusY
        {
            get => GetValue(ViewportZoomFocusYProperty);
            set => SetValue(ViewportZoomFocusYProperty, value);
        }

        public static readonly StyledProperty<double> ViewportZoomFocusYProperty =
            AvaloniaProperty.Register<ZoomAndPanControl, double>
            (
                "ViewportZoomFocusY",
                defaultValue: 0.0
            );

        #endregion Dependency Property Definitions

        #region events

        /// <summary>
        /// Event raised when the ContentOffsetX property has changed.
        /// </summary>
        public event EventHandler ContentOffsetXChanged;

        /// <summary>
        /// Event raised when the ContentOffsetY property has changed.
        /// </summary>
        public event EventHandler ContentOffsetYChanged;

        /// <summary>
        /// Event raised when the ViewportZoom property has changed.
        /// </summary>
        public event EventHandler ContentZoomChanged;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event raised when the 'ViewportZoom' property has changed value.
        /// </summary>
        private void ViewportZoom_PropertyChanged(ZoomAndPanControl c, AvaloniaPropertyChangedEventArgs e)
        {
            //calculate  constant size brush
            (_content as Draw2DControl).UpdateZoomValue(ViewportZoom);
            
            if (c._contentZoomTransform != null)
            {
               
                //
                // Update the content scale transform whenever 'ViewportZoom' changes.
                //
                c._contentZoomTransform.ScaleX = c.ViewportZoom;
                c._contentZoomTransform.ScaleY = c.ViewportZoom;
            }

            //
            // Update the size of the viewport in content coordinates.
            //
            c.UpdateContentViewportSize();

            if (c._enableContentOffsetUpdateFromScale)
            {
                try
                {
                    // 
                    // Disable content focus syncronization.  We are about to update content offset whilst zooming
                    // to ensure that the viewport is focused on our desired content focus point.  Setting this
                    // to 'true' stops the automatic update of the content focus when content offset changes.
                    //
                    c._disableContentFocusSync = true;

                    //
                    // Whilst zooming in or out keep the content offset up-to-date so that the viewport is always
                    // focused on the content focus point (and also so that the content focus is locked to the 
                    // viewport focus point - this is how the google maps style zooming works). 
                    //
                    var viewportOffsetX = c.ViewportZoomFocusX - (c.ViewportWidth / 2);
                    var viewportOffsetY = c.ViewportZoomFocusY - (c.ViewportHeight / 2);
                    var contentOffsetX = viewportOffsetX / c.ViewportZoom;
                    var contentOffsetY = viewportOffsetY / c.ViewportZoom;
                    c.ContentOffsetX = (c.ContentZoomFocusX - (c.ContentViewportWidth / 2)) - contentOffsetX;
                    c.ContentOffsetY = (c.ContentZoomFocusY - (c.ContentViewportHeight / 2)) - contentOffsetY;
                }
                finally
                {
                    c._disableContentFocusSync = false;
                }
            }

            c.ContentZoomChanged?.Invoke(c, EventArgs.Empty);
            c.ScrollOwner?.InvalidateVisual();
            c.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Method called to clamp the 'ViewportZoom' value to its valid range.
        /// </summary>
        private static double ViewportZoom_Coerce(AvaloniaObject o, double baseValue)
        {
            var c = (ZoomAndPanControl)o;
            var value = Math.Max((double)baseValue, c.ContentMinZoom);
            switch (c.MinimumZoomType)
            {
                case MinimumZoomType.FitScreen:
                    value = Math.Min(Math.Max(value, c.ContentFitZoom), c.MaximumZoom);
                    break;
                case MinimumZoomType.FillScreen:
                    value = Math.Min(Math.Max(value, c.ContentFillZoom), c.MaximumZoom);
                    break;
                case MinimumZoomType.MinimumZoom:
                    value = Math.Min(Math.Max(value, c.MinimumZoom), c.MaximumZoom);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return value;
        }

        /// <summary>
        /// Event raised 'MinimumZoom' or 'MaximumZoom' has changed.
        /// </summary>
        private static void MinimumOrMaximumZoom_PropertyChanged(ZoomAndPanControl o,
            AvaloniaPropertyChangedEventArgs e)
        {
            var c = (ZoomAndPanControl)o;
            c.ViewportZoom = Math.Min(Math.Max(c.ViewportZoom, c.MinimumZoom), c.MaximumZoom);
        }

        /// <summary>
        /// Event raised when the 'ContentOffsetX' property has changed value.
        /// </summary>
        private void ContentOffsetX_PropertyChanged(ZoomAndPanControl o, AvaloniaPropertyChangedEventArgs e)
        {
            var c = (ZoomAndPanControl)o;

            c.UpdateTranslationX();

            if (!c._disableContentFocusSync)
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusX();
            //
            // Raise an event to let users of the control know that the content offset has changed.
            //
            c.ContentOffsetXChanged?.Invoke(c, EventArgs.Empty);

            if (!c._disableScrollOffsetSync)
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.ScrollOwner?.InvalidateVisual();
        }

        /// <summary>
        /// Method called to clamp the 'ContentOffsetX' value to its valid range.
        /// </summary>
        private static double ContentOffsetX_Coerce(AvaloniaObject d, double baseValue)
        {
            var c = (ZoomAndPanControl)d;
            var minOffsetX = 0.0;
            var maxOffsetX = Math.Max(0.0, c._unScaledExtent.Width - c._constrainedContentViewportWidth);
            baseValue = Math.Min(Math.Max(baseValue, minOffsetX), maxOffsetX);
            return baseValue;
        }

        /// <summary>
        /// Event raised when the 'ContentOffsetY' property has changed value.
        /// </summary>
        private void ContentOffsetY_PropertyChanged(ZoomAndPanControl o, AvaloniaPropertyChangedEventArgs e)
        {
            var c = (ZoomAndPanControl)o;

            c.UpdateTranslationY();

            if (!c._disableContentFocusSync)
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusY();
            if (!c._disableScrollOffsetSync)
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.ScrollOwner?.InvalidateVisual();
            //
            // Raise an event to let users of the control know that the content offset has changed.
            //
            c.ContentOffsetYChanged?.Invoke(c, EventArgs.Empty);
        }

        /// <summary>
        /// Method called to clamp the 'ContentOffsetY' value to its valid range.
        /// </summary>
        private static double ContentOffsetY_Coerce(AvaloniaObject d, double value)
        {
            var c = (ZoomAndPanControl)d;
            var minOffsetY = 0.0;
            var maxOffsetY = Math.Max(0.0, c._unScaledExtent.Height - c._constrainedContentViewportHeight);
            value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
            return value;
        }

        #endregion

        /// <summary>
        /// Reset the viewport zoom focus to the center of the viewport.
        /// </summary>
        private void ResetViewportZoomFocus()
        {
            ViewportZoomFocusX = ViewportWidth / 2;
            ViewportZoomFocusY = ViewportHeight / 2;
        }

        /// <summary>
        /// Update the viewport size from the specified size.
        /// </summary>
        private void UpdateViewportSize(Size newSize)
        {
            if (_viewport == newSize)
                return;

            _viewport = newSize;

            //
            // Update the viewport size in content coordiates.
            //
            UpdateContentViewportSize();

            //
            // Initialise the content zoom focus point.
            //
            UpdateContentZoomFocusX();
            UpdateContentZoomFocusY();

            //
            // Reset the viewport zoom focus to the center of the viewport.
            //
            ResetViewportZoomFocus();

            //
            // Update content offset from itself when the size of the viewport changes.
            // This ensures that the content offset remains properly clamped to its valid range.
            //
            this.ContentOffsetX = this.ContentOffsetX;
            this.ContentOffsetY = this.ContentOffsetY;

            //
            // Tell that owning ScrollViewer that scrollbar data has changed.
            //
            ScrollOwner?.InvalidateVisual();
        }

        /// <summary>
        /// Update the size of the viewport in content coordinates after the viewport size or 'ViewportZoom' has changed.
        /// </summary>
        private void UpdateContentViewportSize()
        {
            ContentViewportWidth = ViewportWidth / ViewportZoom;
            ContentViewportHeight = ViewportHeight / ViewportZoom;

            _constrainedContentViewportWidth = Math.Min(ContentViewportWidth, _unScaledExtent.Width);
            _constrainedContentViewportHeight = Math.Min(ContentViewportHeight, _unScaledExtent.Height);

            UpdateTranslationX();
            UpdateTranslationY();
        }

        /// <summary>
        /// Update the X coordinate of the translation transformation.
        /// </summary>
        private void UpdateTranslationX()
        {
            if (this._contentOffsetTransform != null)
            {
                var scaledContentWidth = this._unScaledExtent.Width * this.ViewportZoom;
                if (scaledContentWidth < this.ViewportWidth)
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    this._contentOffsetTransform.X = (this.ContentViewportWidth - this._unScaledExtent.Width) / 2;
                else
                    this._contentOffsetTransform.X = -this.ContentOffsetX;
            }
        }

        /// <summary>
        /// Update the Y coordinate of the translation transformation.
        /// </summary>
        private void UpdateTranslationY()
        {
            if (this._contentOffsetTransform != null)
            {
                var scaledContentHeight = this._unScaledExtent.Height * this.ViewportZoom;
                if (scaledContentHeight < this.ViewportHeight)
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    this._contentOffsetTransform.Y = (this.ContentViewportHeight - this._unScaledExtent.Height) / 2;
                else
                    this._contentOffsetTransform.Y = -this.ContentOffsetY;
            }
        }

        /// <summary>
        /// Update the X coordinate of the zoom focus point in content coordinates.
        /// </summary>
        private void UpdateContentZoomFocusX()
        {
            ContentZoomFocusX = ContentOffsetX + (_constrainedContentViewportWidth / 2);
        }

        /// <summary>
        /// Update the Y coordinate of the zoom focus point in content coordinates.
        /// </summary>
        private void UpdateContentZoomFocusY()
        {
            ContentZoomFocusY = ContentOffsetY + (_constrainedContentViewportHeight / 2);
        }

        public double ContentFitZoom => _content == null
            ? Bounds.Width
            : Math.Min((Bounds.Width -10) / _content.Bounds.Width, (Bounds.Height-10) / _content.Bounds.Height);

        public double ContentFillZoom => _content == null
            ? Bounds.Width
            : Math.Max(Bounds.Width / _content.Bounds.Width, Bounds.Height / _content.Bounds.Height);

        public double ContentMinZoom => (MinimumZoomType == MinimumZoomType.FillScreen) ? ContentFillZoom
            : (MinimumZoomType == MinimumZoomType.FitScreen) ? ContentFitZoom
            : MinimumZoom;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}