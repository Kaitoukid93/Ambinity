using Avalonia.Media;
using Draw2D.Core.Handles;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace Draw2D.Core.Policies.CanvasPolicy
{
    public class MousePanPolicy : CanvasPolicy
    {
        
    }

    public class BoundingBoxSelectionPolicy : SingleSelectionPolicy
    {
        private float _x;
        private float _y;
        private Rectangle _selectBox;
        private Geo.Rectangle _absoluteBoundingBox;
        private List<Figure> _figuresInside;

        public override void OnMouseDrag(Canvas canvas, float dxSum, float dySum, float dx, float dy, bool isShiftKey, bool isCtrlKey)
        {
            if (canvas == null || _selectBox == null)
            {
                base.OnMouseDrag(canvas, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey);
                _figuresInside = null;
                return;
            }

            _absoluteBoundingBox.Width += dx;
            _absoluteBoundingBox.Height += dy;
            var normalized = _absoluteBoundingBox.Normalized();

            _selectBox.ForceSetDimensions(normalized);

            _figuresInside = canvas.GetBestFigures(normalized, new List<Type> { typeof(Selectionbox), typeof(IHandle) }, new List<Type>());

            
        }

        public override void OnDragEnd(Canvas canvas, bool isShiftKey, bool isCtrlKey)
        {
            canvas.RemoveAdornerFigure(_selectBox);
            _selectBox = null;

            if (_figuresInside != null)
            {
                //Ignore via policy linked figures. Only master will be selected.
                var slaves = _figuresInside.SelectMany(f => f.Policies.OfType<ILink>())
                    .SelectMany(f => f.GetLinkedFigures()).ToList();

                slaves.ForEach(s => _figuresInside.Remove(s));

                canvas.Selection.All.Where(f => !_figuresInside.Contains(f))
                    .ToList()
                    .ForEach(f => Unselect(canvas, f));

                foreach (var figure in _figuresInside.Where(f => f.IsSelectable))
                {
                    Select(canvas, figure);
                }
            }
            base.OnDragEnd(canvas, isShiftKey, isCtrlKey);
        }

        public override void OnDragStart(Canvas canvas, float startPosX, float startPosY, bool isShiftKey,
            bool isCtrlKey)
        {
            var hitFigure = canvas.GetBestFigure(startPosX, startPosY, new List<Type> { typeof(Selectionbox) },
                new List<Type>());

            if (hitFigure != null && hitFigure.IsSelectable)
            {
                base.OnDragStart(canvas, startPosX, startPosY, isShiftKey, isCtrlKey);
                return;
            }

            _x = startPosX;
            _y = startPosY;

            //Console.WriteLine($"Drag start: {_x}, {_y}");
            _selectBox = new Selectionbox(_x, _y, 1, 1);
            _selectBox.FillColor = canvas.StrokeColor.AdjustOpacity(0.1);
            _selectBox.StrokeColor = canvas.StrokeColor;
            _absoluteBoundingBox = _selectBox.BoundingBox.Clone();

            canvas.AddAdornerFigure(_selectBox);
        }




    }
}
