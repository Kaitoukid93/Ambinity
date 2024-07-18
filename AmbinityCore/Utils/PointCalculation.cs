using Avalonia;

namespace AmbinityCore.Utils;

public class PointCalculation
{
    public static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
    {
        double angleInRadians = angleInDegrees * (Math.PI / 180);
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        var x = (cosTheta * (pointToRotate.X - centerPoint.X) -
            sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X);
        var y = (sinTheta * (pointToRotate.X - centerPoint.X) +
                 cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y);
            return new Point(x, y);

    }
    public static Point ReflectPointVertical(Point pointToReflect, double center)
    {
        double distance = pointToReflect.X - center;
        return new Point(center - distance, pointToReflect.Y);
    }
}