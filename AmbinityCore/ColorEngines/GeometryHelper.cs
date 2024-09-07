using Avalonia;

namespace AmbinityCore.LightingEngines;

/// <summary>
/// Copilot lol...
/// </summary>
public class GeometryHelper
{
    public static List<Point> FindIntersectingPixels(Point[] polygon, double m, double b)
    {
        List<Point> intersectingPixels = new List<Point>();

        // Get the bounding box of the polygon
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var point in polygon)
        {
            if (point.X < minX) minX = (int)point.X;
            if (point.Y < minY) minY = (int)point.Y;
            if (point.X > maxX) maxX = (int)point.X;
            if (point.Y > maxY) maxY = (int)point.Y;
        }

        // Use Bresenham's line algorithm to find all pixels the line passes through
        List<Point> linePixels = BresenhamLine(minX, (int)(m * minX + b), maxX, (int)(m * maxX + b));

        // Check each pixel to see if it lies within the polygon
        foreach (var pixel in linePixels)
        {
            if (IsPointInPolygon(polygon, pixel))
            {
                intersectingPixels.Add(pixel);
            }
        }

        return intersectingPixels;
    }

    public static List<Point> FindIntersectingPixels(Point[] polygon, int c)
    {
        List<Point> intersectingPixels = new List<Point>();

        // The vertical line x = -c
        int x = c;

        // Get the bounding box of the polygon
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var point in polygon)
        {
            if (point.Y < minY) minY = (int)point.Y;
            if (point.Y > maxY) maxY = (int)point.Y;
        }

        // Use Bresenham's line algorithm to find all pixels the line passes through
        List<Point> linePixels = BresenhamLine(x, minY, x, maxY);

        // Check each pixel to see if it lies within the polygon
        foreach (var pixel in linePixels)
        {
            if (IsPointInPolygon(polygon, pixel))
            {
                intersectingPixels.Add(pixel);
            }
        }

        return intersectingPixels;
    }

    private static List<Point> BresenhamLine(int x0, int y0, int x1, int y1)
    {
        List<Point> points = new List<Point>();

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            points.Add(new Point(x0, y0));
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return points;
    }

    private static bool IsPointInPolygon(Point[] polygon, Point point)
    {
        bool result = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].Y <= point.Y && polygon[j].Y > point.Y || polygon[j].Y <= point.Y && polygon[i].Y > point.Y)
            {
                if (polygon[i].X + (point.Y - polygon[i].Y) / (double)(polygon[j].Y - polygon[i].Y) *
                    (polygon[j].X - polygon[i].X) <= point.X)
                {
                    result = !result;
                }
            }

            j = i;
        }

        return result;
    }

    public static Point[] RectToPolygon(Rect rect)
    {
        Point[] polygonPoints = new Point[4];
        polygonPoints[0] = new Point(rect.Left, rect.Top);
        polygonPoints[1] = new Point(rect.Right, rect.Top);
        polygonPoints[2] = new Point(rect.Right, rect.Bottom);
        polygonPoints[3] = new Point(rect.Left, rect.Bottom);
        return polygonPoints;
    }

    /// <summary>
    /// Take in polygon data with angle and return list of lines, each lines is a 1d array of pixels
    /// </summary>
    /// <param name="bound"></param>
    /// <param name="polygon"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static List<Point[]> GetPixelsLineFromPolygonWithAngle(Rect bound, Point[] polygon, int angle)
    {
        List<Point[]> lines = new List<Point[]>();
        if (angle == 90 || angle == -90)
        {
            for (int i = (int)bound.X; i < bound.Width + bound.X; i++)
            {
                var pixels = FindIntersectingPixels(polygon, i);
                if (pixels.Count > 0)
                    lines.Add(pixels.ToArray());
            }
        }
        else
        {
            var m = Math.Tan(angle * (Math.PI / 180));
            var absm = Math.Abs(m);
            int _yOffset = (int)bound.Y;
            int start = (int)(_yOffset - (bound.Width + bound.X) * absm);
            int end = (int)(_yOffset + bound.Height + (bound.Width + bound.X) * absm);
            for (int i = start; i < end; i++)
            {
                var pixels = FindIntersectingPixels(polygon, m, i);
                if (pixels.Count > 0)
                    lines.Add(pixels.ToArray());
            }
        }

        return lines;
    }

    public static List<Point[]> GetpixelsLineFromListOfRectangleWidthAngle(Rect bound, List<Rect> rects, int angle)
    {
        List<Point[]> lines = new List<Point[]>();
        if (angle == 90 || angle == -90)
        {
            for (int i = (int)bound.X; i < bound.Width + bound.X; i++)
            {
                var line = new List<Point>();
                foreach (var rect in rects)
                {
                    var poly = RectToPolygon(rect);
                    var pixels = FindIntersectingPixels(poly, i);
                    if (pixels.Count > 0)
                        line.AddRange(pixels);
                }
                if (line.Count > 0)
                    lines.Add(line.ToArray());
            }
        }
        else
        {
            var m = Math.Tan(angle * (Math.PI / 180));
            var absm = Math.Abs(m);
            int _yOffset = (int)bound.Y;
            int start = (int)(_yOffset - (bound.Width + bound.X) * absm);
            int end = (int)(_yOffset + bound.Height + (bound.Width + bound.X) * absm);
            for (int i = start; i < end; i++)
            {
                var line = new List<Point>();
                foreach (var rect in rects)
                {
                    var poly = RectToPolygon(rect);
                    var pixels = FindIntersectingPixels(poly, m, i);
                    if (pixels.Count > 0)
                        line.AddRange(pixels);
                }

                if (line.Count > 0)
                    lines.Add(line.ToArray());
            }
        }

        return lines;
    }

    /// <summary>
    /// Take in polyline data with thickness and return list of pixels that form a stroke
    /// </summary>
    /// <param name="polyline"></param>
    /// <param name="thickness"></param>
    /// <returns></returns>
    public static List<Point[]> GetPixelsFromPolylineWithThickness(Point[] polyline, int thickness)
    {
        List<Point[]> pixels = new List<Point[]>();

        for (int i = 0; i < polyline.Length - 1; i++)
        {
            Point start = polyline[i];
            Point end = polyline[i + 1];
            pixels.AddRange(BresenhamLineWithThickness((int)start.X, (int)start.Y, (int)end.X, (int)end.Y, thickness));
        }

        if (polyline.Length > 2)
        {
            pixels.AddRange(BresenhamLineWithThickness((int)polyline[polyline.Length - 1].X,
                (int)polyline[polyline.Length - 1].Y,
                (int)polyline[0].X, (int)polyline[0].Y, thickness));
        }
       
        return pixels;
    }

    public static void RemoveDuplicates<T>(IList<T> list)
    {
        if (list == null)
        {
            return;
        }

        int i = 1;
        while (i < list.Count)
        {
            int j = 0;
            bool remove = false;
            while (j < i && !remove)
            {
                if (list[i].Equals(list[j]))
                {
                    remove = true;
                }

                j++;
            }

            if (remove)
            {
                list.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    private static List<Point[]> BresenhamLineWithThickness(int x0, int y0, int x1, int y1, int thickness)
    {
        List<Point[]> line = new List<Point[]>();

        int dx = (int)Math.Abs(x1 - x0);
        int dy = (int)Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        List<Point> lastPoints = new List<Point>();
        while (true)
        {
            List<Point> points = new List<Point>();
            for (int i = -thickness / 2; i <= thickness / 2; i++)
            {
                for (int j = -thickness / 2; j <= thickness / 2; j++)
                {
                    var newPoint = new Point(x0 + i, y0 + j);
                    if (!lastPoints.Contains(newPoint))
                    {
                        lastPoints.Add(newPoint);
                        points.Add(newPoint);
                    }
                }
            }

            line.Add(points.ToArray());
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return line;
    }

    /// <summary>
    /// Return list of line(Points[]) that form an ellipse
    /// </summary>
    /// <param name="center"></param>
    /// <param name="rx"></param>
    /// <param name="ry"></param>
    /// <param name="m"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static List<Point[]> GetPixelInsideEllipseWithAngle(Rect bound, Point center, int rx, int ry, int angle)
    {
        List<Point[]> lines = new List<Point[]>();

        if (angle == 90 || angle == -90)
        {
            for (int i = (int)bound.X; i < bound.Width + bound.X; i++)
            {
                var pixels = FindIntersectingPixels(center, rx, ry, i);
                if (pixels.Count > 0)
                    lines.Add(pixels.ToArray());
            }
        }
        else
        {
            var m = Math.Tan(angle * (Math.PI / 180));
            var absm = Math.Abs(m);
            int _yOffset = (int)bound.Y;
            int start = (int)(_yOffset - (bound.Width + bound.X) * absm);
            int end = (int)(_yOffset + bound.Height + (bound.Width + bound.X) * absm);
            for (int i = start; i < end; i++)
            {
                var pixels = FindIntersectingPixels(center, rx, ry, m, i);
                if (pixels.Count > 0)
                    lines.Add(pixels.ToArray());
            }
        }

        return lines;
    }

    public static List<Point> FindIntersectingPixels(Point center, int rx, int ry, double m, double b)
    {
        List<Point> intersectingPixels = new List<Point>();

        // Get the bounding box of the ellipse
        int minX = (int)center.X - rx;
        int maxX = (int)center.X + rx;
        int minY = (int)center.Y - ry;
        int maxY = (int)center.Y + ry;

        // Use Bresenham's line algorithm to find all pixels the line passes through
        List<Point> linePixels = BresenhamLine(minX, (int)(m * minX + b), maxX, (int)(m * maxX + b));

        // Check each pixel to see if it lies within the ellipse
        foreach (var pixel in linePixels)
        {
            if (IsPointInEllipse(center, rx, ry, pixel))
            {
                intersectingPixels.Add(pixel);
            }
        }

        return intersectingPixels;
    }

    public static List<Point> FindIntersectingPixels(Point center, int rx, int ry, int c)
    {
        List<Point> intersectingPixels = new List<Point>();

        // The vertical line x = -c
        int x = c;

        // Get the bounding box of the polygon
        int minY = (int)center.Y - ry;
        int maxY = (int)center.Y + ry;

        // Use Bresenham's line algorithm to find all pixels the line passes through
        List<Point> linePixels = BresenhamLine(x, minY, x, maxY);

        // Check each pixel to see if it lies within the polygon
        foreach (var pixel in linePixels)
        {
            if (IsPointInEllipse(center, rx, ry, pixel))
            {
                intersectingPixels.Add(pixel);
            }
        }

        return intersectingPixels;
    }

    private static bool IsPointInEllipse(Point center, int rx, int ry, Point point)
    {
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;
        return (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1;
    }

    public static List<Point> ConvertEllipseToPolygon(Point center, float rx, float ry, float edgeSize)
    {
        // Approximate the circumference of the ellipse
        double a = rx;
        double b = ry;
        double circumference = Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));

        // Determine the number of points
        int numberOfPoints = (int)(circumference / edgeSize);

        List<Point> polygonPoints = new List<Point>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            double angle = 2 * Math.PI * i / numberOfPoints;
            double x = center.X + rx * (float)Math.Cos(angle);
            double y = center.Y + ry * (float)Math.Sin(angle);
            polygonPoints.Add(new Point(x, y));
        }

        return polygonPoints;
    }
}