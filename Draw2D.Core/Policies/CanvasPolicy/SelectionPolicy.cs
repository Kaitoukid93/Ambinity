namespace Draw2D.Core.Policies.CanvasPolicy
{
    public abstract class SelectionPolicy : CanvasPolicy
    {
        public abstract void Select(Canvas canvas, Figure figure);
        public abstract void Select(Canvas canvas, List<Figure> figures);
        public virtual void Unselect(Canvas canvas, Figure figure, bool notify)
        {
            canvas.Selection.Remove(figure);
        }
      
    }
}