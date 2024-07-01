namespace Draw2D.Core.Shapes.Basic
{
    public class Selectionbox : Rectangle
    {
        public Selectionbox(float x, float y, float width, float height) : base(x, y, width, height)
        {
      
        }

        public Selectionbox(Geo.Rectangle rect) : base(rect)
        {
        }
    }
}
