using System;
using System.Collections.Generic;
using System.Linq;
using Draw2D.Core.Geo;
using Draw2D.Core.Handles;
using Draw2D.Core.Shapes.Basic;

namespace Draw2D.Core.Policies.CanvasPolicy
{
    public class SingleSelectionPolicy : SelectionPolicy, IMouseAware, IDragAware
    {
        private bool _mouseMovedDuringMouseDown;
        private Figure _mouseDraggingElement;
        private Figure _mouseDownElement;


        public virtual void OnClick(Figure figure, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            throw new NotImplementedException();
        }

        public virtual void OnDoubleClick(Figure figure, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            throw new NotImplementedException();
        }

        public virtual void OnMouseMove(Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            var figure =
                canvas.GetBestFigure(mouseX, mouseY, new List<Type> { typeof(Selectionbox) }, new List<Type>());
            if (figure == null)
            {
                canvas.UnHoverAll();
                return;
            }

            if (figure.IsSelectable == true)
            {
                canvas?.HoverFigure(figure);
            }
            else
            {
                canvas?.UnHoverAll();
                return;
            }
        }

        public virtual void OnMouseLeftDown(Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            _mouseMovedDuringMouseDown = false;
            _mouseDownElement = null;
            var figure =
                canvas.GetBestFigure(mouseX, mouseY, new List<Type> { typeof(Selectionbox) }, new List<Type>());

            if (figure == null)
            {
                Unselect(canvas, canvas.Selection.All);

                return;
            }

            if (figure.IsSelectable == false)
            {
                Unselect(canvas, canvas.Selection.All);
                return;
            }
            _mouseDownElement = figure;
            if (canvas.Selection.Contains(figure))
            {
                return;
            }

            //Ignore via policy linked figures. Only master will be selected.
            var slaves = canvas.Selection.All.SelectMany(f => f.Policies.OfType<ILink>())
                .SelectMany(f => f.GetLinkedFigures()).ToList();
            Unselect(canvas, slaves);

            //Unselect the slaves of the newly selected figure.
            var slavesOfToBeSelected = figure.Policies.OfType<ILink>().SelectMany(f => f.GetLinkedFigures()).ToList();
            Unselect(canvas, slavesOfToBeSelected);

            if (isShiftKey)
            {
                if (canvas.Selection.Contains(figure))
                {
                    Unselect(canvas, figure);
                }
                else if (figure.IsSelectable)
                {
                    Select(canvas, figure);
                }
            }
            else
            {
                //Todo:Select Problem ->  isDragging

                if (figure is ResizeHandle)
                    Select(canvas, (figure as ResizeHandle).Owner);
                else
                {
                    Unselect(canvas, canvas.Selection.All);
                    Select(canvas, figure);
                }
            }
        }

        public virtual void OnMouseLeftDoubleClick(Canvas canvas, float mouseX, float mouseY, bool isShiftKey,
            bool isCtrlKey)
        {
        }


        public virtual void OnMouseDrag(Canvas canvas, float dxSum, float dySum, float dx, float dy, bool isShiftKey,
            bool isCtrlKey)
        {
            if (canvas == null)
                return;

            if (!canvas.Selection.All.Any())
            {
                //Panning of canvas because there is no selection to drag.
            }
            else
            {
                if (isCtrlKey && _mouseDownElement != null)
                {
                    _mouseDownElement.OnDrag(canvas, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey);
                }
                else if (canvas.Selection.All.Count() == 1)
                {
                    canvas.Selection.All.ToList()
                        .ForEach(f => f.OnDrag(canvas, dxSum, dySum, dx, dy, isShiftKey, isCtrlKey));
                }
                else
                {
                    Point delta = new Point(dx, dy);
                    bool isSnapped = false;

                    foreach (var snapPolicy in canvas.GetSnapPolicies())
                    {
                        Point snapPoint;
                        isSnapped = snapPolicy.Snap(canvas, canvas.Selection.All.First().Position, dx, dy, dxSum, dySum,
                            out snapPoint, out delta, canvas.Selection.All);

                        if (isSnapped)
                            break;
                    }

                    if (isSnapped)
                    {
                        dx = delta.X;
                        dy = delta.Y;
                    }

                    canvas.Selection.All.ToList().ForEach(f => f.Translate(dx, dy, canvas.Selection.All.Count == 1));
                }
            }
        }

        public virtual void OnDragStart(Canvas canvas, float startPosX, float startPosY,
            bool isShiftKey, bool isCtrlKey)
        {
            if (canvas == null)
                return;

            if (!canvas.Selection.All.Any())
            {
                //Panning of canvas because there is no selection to drag.
                var x = 5;
            }
            else
            {
                if (isCtrlKey && _mouseDownElement != null)
                {
                    _mouseDownElement.OnDragStart(canvas, startPosX, startPosY);
                }
                else if (canvas.Selection.All.Count() == 1)
                {
                    canvas.Selection.All.ToList().ForEach(f => f.OnDragStart(canvas, startPosX, startPosY));
                }
                else
                {
                    foreach (var snapPolicy in canvas.GetSnapPolicies())
                    {
                        snapPolicy.InitSnap(canvas, canvas.Selection.All.SelectMany(f => f.GetSnapPoints()),
                            canvas.Selection.All);
                    }
                }

                //Do not forget the minimal drag distance already dragged.
                //canvas.Selection.All.ToList().ForEach(f => f.OnDrag(canvas,dx, dy, dx, dy, isShiftKey, isCtrlKey));
            }
        }

        public virtual void OnDragEnd(Canvas canvas, bool isShiftKey, bool isCtrlKey)
        {
            if (canvas == null)
                return;

            if (!canvas.Selection.All.Any())
            {
                //Panning of canvas because there is no selection to drag.
                var x = 5;
            }
            else
            {
                if (canvas.Selection.All.Count() == 1)
                {
                    foreach (var selectedFigure in canvas.Selection.All)
                    {
                        selectedFigure.ShowHandles(canvas);
                    }
                }
                else if (canvas.Selection.All.Count() > 1)
                {
                    foreach (var snapPolicy in canvas.GetSnapPolicies())
                    {
                        snapPolicy.EndSnapping(canvas);
                    }
                }

                canvas.Selection.All.ToList().ForEach(f => f.OnDragEnd(canvas, isShiftKey, isCtrlKey));
            }

            canvas.NeedsRepaint(null);
        }

        public virtual void OnMouseLeftUp(Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            if (canvas.Selection.All.Count() == 1)
            {
                foreach (var selectedFigure in canvas.Selection.All)
                {
                    selectedFigure.ShowHandles(canvas);
                }
            }
            else if (canvas.Selection.All.Count() > 1)
            {
                foreach (var selectedFigure in canvas.Selection.All)
                {
                    selectedFigure.HideHandles(canvas);
                }
            }
            if (_mouseDownElement != null&&!isShiftKey)
            {
                Unselect(canvas, canvas.Selection.All);
                _mouseDownElement.Select();
                _mouseDownElement = null;
            }
        }

        public virtual void OnMouseRightDown(Canvas canvas, float mouseX, float mouseY, bool isShiftKey, bool isCtrlKey)
        {
            _mouseMovedDuringMouseDown = false;

            var figure =
                canvas.GetBestFigure(mouseX, mouseY, new List<Type> { typeof(Selectionbox) }, new List<Type>());

            if (figure == null)
            {
                Unselect(canvas, canvas.Selection.All);
                return;
            }

            if (figure.IsSelectable == false)
            {
                Unselect(canvas, canvas.Selection.All);
            }

            if (canvas.Selection.Contains(figure))
            {
                return;
            }

            //Ignore via policy linked figures. Only master will be selected.
            var slaves = canvas.Selection.All.SelectMany(f => f.Policies.OfType<ILink>())
                .SelectMany(f => f.GetLinkedFigures()).ToList();
            Unselect(canvas, slaves);

            //Unselect the slaves of the newly selected figure.
            var slavesOfToBeSelected = figure.Policies.OfType<ILink>().SelectMany(f => f.GetLinkedFigures()).ToList();
            Unselect(canvas, slavesOfToBeSelected);

            if (isShiftKey)
            {
                if (canvas.Selection.Contains(figure))
                {
                    Unselect(canvas, figure);
                }
                else if (figure.IsSelectable)
                {
                    Select(canvas, figure);
                }
            }
            else
            {
                //Todo:Select Problem ->  isDragging
                Unselect(canvas, canvas.Selection.All);
                Select(canvas, figure);
            }
        }

        public override void Select(Canvas canvas, Figure figure)
        {
            if (figure == null || canvas == null)
                return;

            if (!figure.IsSelectable)
                return;

            if (canvas.Selection.Contains(figure))
                return;


            figure.Select(true, false);
            canvas.Selection.Primary = figure;
        }

        public override void Select(Canvas canvas, List<Figure> figures)
        {
            if (figures == null || canvas == null)
                return;
            var touchable = new List<Figure>();
            foreach (var figure in figures)
            {
                if (figure.IsSelectable && !canvas.Selection.Contains(figure))
                    touchable.Add(figure);
            }

            //only notify when last figure is selected
            for (int i = 0; i < touchable.Count; i++)
            {
                if (i == touchable.Count - 1)
                {
                    touchable[i].Select(true, false);
                }
                else
                {
                    touchable[i].Select(true, false, false);
                }
            }

            //canvas.Selection.Primary = touchable?[0];
        }

        public override void Unselect(Canvas canvas, Figure figure, bool notify = true)
        {
            if (figure == null)
                return;

            if (!figure.IsSelectable)
                return;
            figure.Unselect();
            canvas.Selection.Remove(figure, notify);

        }

        private void Unselect(Canvas canvas, IEnumerable<Figure> all)
        {
            var newList = new List<Figure>(all);

            for (var i = 0; i < newList.Count; i++)
            {
                Unselect(canvas, newList[i], i >= newList.Count - 1);
            }
        }

        private void MouseLeave(Figure figure)
        {
            figure.IsMouseOver = false;
        }

        private void MouseEnter(Figure figure)
        {
            figure.IsMouseOver = true;
        }
    }
}
