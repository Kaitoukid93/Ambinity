using Avalonia;
using SkiaSharp;

namespace AmbinityCore.Utils;

public class RectCalculation
{
    /// <summary>
    /// Get bound of SKRects
    /// </summary>
    /// <param name="rects"></param>
    /// <returns></returns>
        public static Rect GetBound(Rect[] rects)
        {
            double xMin = rects.Min(s => s.Left);
            double yMin = rects.Min(s => s.Top);
            double xMax = rects.Max(s => s.Left + s.Width);
            double yMax = rects.Max(s => s.Top + s.Height);
            var rect = new Rect(xMin - 1, yMin - 1, xMax + 1, yMax + 1);
            return rect;
        }
 
       
        public static SKRect RotateRectangle(SKRect inputRect, Point centerPoint, double angleInDegrees)
        {
            //rotate 4 point
            var topleft = new Point(inputRect.Top, inputRect.Left);
            var topright = new Point(inputRect.Top, inputRect.Right);
            var bottomleft = new Point(inputRect.Bottom, inputRect.Left);
            var bottomRight = new Point(inputRect.Bottom, inputRect.Right);
            var newTopLeft = PointCalculation.RotatePoint(topleft, centerPoint, angleInDegrees);
            var newTopRight = PointCalculation.RotatePoint(topright, centerPoint, angleInDegrees);
            var newBottomLeft = PointCalculation.RotatePoint(bottomleft, centerPoint, angleInDegrees);
            var newTopBottomRight = PointCalculation.RotatePoint(bottomRight, centerPoint, angleInDegrees);
            var listPoint = new List<Point>();
            listPoint.Add(newTopLeft); listPoint.Add(newTopRight); listPoint.Add(newBottomLeft); listPoint.Add(newTopBottomRight);
            double minX = listPoint.Min(p => p.X);
            double minY = listPoint.Min(p => p.Y);
            double maxX = listPoint.Max(p => p.X);
            double MaxY = listPoint.Max(p => p.Y);
            //get boundingBox
            var rect = new SKRect((float)minX, (float)minY, (float)maxX, (float)MaxY);
            return rect;
        }
    
}