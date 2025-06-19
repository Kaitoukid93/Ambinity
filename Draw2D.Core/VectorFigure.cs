using Avalonia.Media;
using Avalonia.Platform;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Utlils;

namespace Draw2D.Core
{
    public abstract class VectorFigure : Figure
    {
        private Canvas _canvas;
        private IDashStyle _storedDashStyle;
        private Color _storedFillColor;
        private Color _strokeColor = Colors.Green;
        private float _strokeThickness = 2;
        private IDashStyle _dashStyle = null;
        private Color _fillColor = Colors.Transparent;
        private bool _overideStrokeStyle = true;
        public event Action StrokeThicknessChanged;
        public Color FillColor
        {
            get { return _fillColor; }
            set
            {
                _fillColor = value;
                Canvas?.NeedsRepaint(this);
            }
        }

        /// <summary>
        /// set this to true if you want the shape being rendered using user defined stroke style
        /// set this to fail if you want the shape being rendered using global stroke style (primary color, constant thickness)
        /// </summary>
        public bool OverrideStrokeStyle
        {
            get { return _overideStrokeStyle; }
            set { _overideStrokeStyle = value; }
        }

        public Color StrokeColor
        {
            get { return _strokeColor; }
            set
            {
                _strokeColor = value;
                Canvas?.NeedsRepaint(this);
            }
        }

        public float StrokeThickness
        {
            get { return _strokeThickness; }
            set
            {
                _strokeThickness = value;
                StrokeThicknessChanged?.Invoke();
                Canvas?.NeedsRepaint(this);
            }
        }

        public IDashStyle DashStyle
        {
            get { return _dashStyle; }
            set
            {
                _dashStyle = value;
                Canvas?.NeedsRepaint(this);
            }
        }

        public override Figure EnableSelectionFeedback(bool isFeedbackEnabled)
        {
            if (isFeedbackEnabled)
            {
                _storedDashStyle = DashStyle;
                _storedFillColor = FillColor;
                FillColor = Colors.White.AdjustOpacity(0.2);
                DashStyle = Avalonia.Media.DashStyle.Dash;
            }
            else
            {
                FillColor = _storedFillColor;
                DashStyle = _storedDashStyle;
            }


            Canvas?.NeedsRepaint(this);

            return this;
        }

        public virtual Figure EnableDragFeedback(bool isFeedbackEnabled)
        {
            if (isFeedbackEnabled)
            {
                _storedDashStyle = DashStyle;
                _storedFillColor = FillColor;
                FillColor = FillColor.AdjustOpacity(0.2);
            }
            else
            {
                FillColor = _storedFillColor;
            }


            Canvas?.NeedsRepaint(this);

            return this;
        }


        public override void OnDragEnd(Canvas canvas, bool isShiftKey, bool isCtrlKey)
        {
            base.OnDragEnd(canvas, isShiftKey, isCtrlKey);

            foreach (var policy in Policies.OfType<DragFeedbackPolicy>())
            {
                policy.OnDragEnd(canvas, this, isShiftKey, isCtrlKey);
            }
        }

        public override void OnDrag(Canvas canvas, float dxSum, float dySum, float dx, float dy, bool isShiftKey,
            bool isCtrlKey)
        {
            base.OnDrag(canvas, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey);

            foreach (var policy in Policies.OfType<DragFeedbackPolicy>())
            {
                policy.OnDrag(canvas, this, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey);
            }
        }


        public override Canvas Canvas
        {
            get { return _canvas; }
            set
            {
                if (value == null)
                {
                    Unselect();
                }

                _canvas = value;
            }
        }

        public abstract void Render(DrawingContext dc, double strokeThickness, Color strokeColor);

    }
}
