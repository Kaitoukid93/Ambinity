namespace Draw2D.Core
{
    public class TopDownCartesianCoordinateSystem : ICoordinateSystem
    {
        public float OffsetX { get;  }
        public float OffsetY { get;   }
       

        public TopDownCartesianCoordinateSystem(float offsetX, float offsetY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
       
        public Geo.Point ToWorldSpace(double screenPointX, double screenPointY)
        {
            var factor = 1; //internally everthing is stored in 1/100.
            return new Geo.Point((float)Math.Ceiling(screenPointX - OffsetX * factor),
                (float)Math.Ceiling(screenPointY - OffsetY * factor));
        }

        public double[] ToScreenSpace(Geo.Point worldPoint)
        {
            var factor = 1; //internally everthing is stored in 1/100.
            return new double[]
            {
                worldPoint.X + OffsetX  / factor,
                worldPoint.Y + OffsetY  / factor
            };
        }

        public ICanvas Canvas { get; set; }
    }
}
