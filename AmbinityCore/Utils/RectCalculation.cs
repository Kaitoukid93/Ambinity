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
        var rect = new Rect(xMin , yMin , xMax - xMin , yMax - yMin);
        return rect;
    }

    /// <summary>
    /// Translate a rect from originalRect coordinate to newRect coordinate
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="originalRect"></param>
    /// <param name="newRect"></param>
    /// <returns></returns>
    public static Rect TranslateRect(Rect rect, Rect originalRect, Rect newRect)
    {
        var r = rect;
        var offsetX = rect.X - originalRect.X < 0 ? 0 : rect.X - originalRect.X;
        var offsetY = rect.Y - originalRect.Y < 0 ? 0 : rect.Y - originalRect.Y;
        var ratioX = offsetX / originalRect.Width;
        var ratioY = offsetY / originalRect.Height;
        var ratioWidth = rect.Width / originalRect.Width;
        var ratioHeight = rect.Height / originalRect.Height;
        r = new Rect(ratioX * newRect.Width, ratioY * newRect.Height,
            ratioWidth * newRect.Width < 2 ? 2 : ratioWidth * newRect.Width,
            ratioHeight * newRect.Height < 2 ? 2 : ratioHeight * newRect.Height);
        return r;
    }

    public static Rect TransformRectangle(Rect rect, Point centerPoint, double andgleInDegree, double scale, double x,
        double y)
    {
        Matrix translateToOrigin = Matrix.CreateTranslation(-centerPoint.X, -centerPoint.Y);
        Matrix translateBack = Matrix.CreateTranslation(centerPoint.X, centerPoint.Y);
        var angleInRadians = andgleInDegree * (Math.PI / 180);
        // Create rotation matrix
        Matrix rotationMatrix = Matrix.CreateRotation(angleInRadians);
        Matrix scaleMatrix = Matrix.CreateScale(scale, scale);
        Matrix offset = Matrix.CreateTranslation(x, y);
        // Combine transformations
        Matrix transformMatrix = scaleMatrix * translateToOrigin * rotationMatrix * translateBack * offset;

        // Apply transformation to each corner of the rectangle
        Point topLeft = transformMatrix.Transform(new Point(rect.X, rect.Y));
        Point topRight = transformMatrix.Transform(new Point(rect.X + rect.Width, rect.Y));
        Point bottomLeft = transformMatrix.Transform(new Point(rect.X, rect.Y + rect.Height));
        Point bottomRight = transformMatrix.Transform(new Point(rect.X + rect.Width, rect.Y + rect.Height));

        // Calculate the bounding box of the rotated rectangle
        double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
        double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
        double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
        double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

        Rect boundingBox = new Rect(minX, minY, maxX - minX, maxY - minY);
        return boundingBox;
    }
}