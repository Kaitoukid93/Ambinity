using AmbinityCore.Models.Device.LED;
using Avalonia;
using Draw2D.Core;
using Draw2D.Core.Shapes.Basic;
using SkiaSharp;

namespace AmbinityCore.Utils;

public class GeometryUltilities
{
    public static Figure? CombineLED(List<LEDContainerFigure> figures)
    {


        if (figures.Count <= 1)
            return null;

       var boundingBox = GetBoundingBox(figures);

        var combinedPath = new SKPath();

        foreach (var figure in figures)
        {
            if (figure.ChildItem is AmbinityLED led && !string.IsNullOrEmpty(led.Geometry))
            {
                var path = SKPath.ParseSvgPathData(led.Geometry);
                if (path != null)
                {
                    // Move path to origin
                    var toOrigin = SKMatrix.CreateTranslation(-path.Bounds.Left, -path.Bounds.Top);

                    // Scale to container size
                    float scaleX = figure.Width / path.Bounds.Width;
                    float scaleY = figure.Height / path.Bounds.Height;
                    var scale = SKMatrix.CreateScale(scaleX, scaleY);

                    // Translate to container position
                    var left = figure.X - boundingBox.X;
                    var top = figure.Y - boundingBox.Y;
                    var translation = SKMatrix.CreateTranslation((float)left, (float)top);

                    // Combine matrices: toOrigin -> scale -> translation
                    var temp = SKMatrix.Concat(toOrigin, translation);
                    var finalMatrix = SKMatrix.Concat(temp, scale);

                    combinedPath.AddPath(path, ref finalMatrix);
                }
            }
        }

        if (combinedPath.IsEmpty)
            return null;

        string geometryString = combinedPath.ToSvgPathData();
        var fig = CreateLED(geometryString, boundingBox);
        return fig;
    }

    private static Figure? CreateLED(string geometryString, Draw2D.Core.Geo.Rectangle boundingBox)
    {
        var led = new AmbinityLED(new ArgbLed(),
                     null,
                      (float)boundingBox.X,
                      (float)boundingBox.Y,
                       (float)boundingBox.Width,
                        (float)boundingBox.Height,
                         0,
                          false,
                           geometryString);
        led.X = (float)boundingBox.X;
        led.Y = (float)boundingBox.Y;
        var containerFigure = led.GetContainer();
        containerFigure.SetChild(led);
        containerFigure.MinHeight = 5;
        containerFigure.MinWidth = 5;
        return containerFigure;
    }

    public static Figure? CreateLEDFromSelectedPolyLines(List<PolyLine> figures)
    {

        if (figures.Count == 0)
            return null;
        var boundingBox = GetBoundingBox(figures);
        var skPath = new SKPath();

        foreach (var fig in figures)
        {
            if (fig is PolyLine polyline)
            {
                var points = polyline.Points.ToList();
                if (points.Count < 2)
                    continue;

                // Auto-close if not closed
                if (points.First() != points.Last())
                    points.Add(points.First());

                // Convert points to screen space and SKPoint
                var skPoints = points
                    .Select(p =>
                    {
                        var vertex = polyline.Canvas.CoordinateSystem.ToScreenSpace(p);
                        return new SKPoint((float)vertex[0], (float)vertex[1]);
                    })
                    .ToList();

                // Build the path for this polyline
                skPath.MoveTo(skPoints[0]);
                for (int i = 1; i < skPoints.Count; i++)
                {
                    skPath.LineTo(skPoints[i]);
                }
                skPath.Close();
            }

        }

        // If no path was created, return
        if (skPath.IsEmpty)
            return null;
        string geometryString = skPath.ToSvgPathData();
        var led = CreateLED(geometryString, boundingBox);
        return led;
    }

    public static string? SplitGeometry(string geometryString, int nRows = 0, int nCols = 0, float colGap = 0, float rowGap = 0)
    {
        var path = SKPath.ParseSvgPathData(geometryString);
        if (path == null)
            return geometryString;
        var scaleX = 1;
        var scaleY = 1;
        var left = 0;
        var top = 0;
        var toOrigin = SKMatrix.CreateTranslation(-path.Bounds.Left, -path.Bounds.Top);
        var translate = SKMatrix.CreateTranslation(left, top);
        var scale = SKMatrix.CreateScale(scaleX, scaleY);
        var temp = SKMatrix.Concat(toOrigin, translate);
        var finalMatrix = SKMatrix.Concat(temp, scale);
        path.Transform(finalMatrix);
        // Calculate total gap space
        float totalColGap = (nCols - 1) * colGap;
        float totalRowGap = (nRows - 1) * rowGap;
        // Calculate cell size with gaps
        float cellWidth = (path.Bounds.Width - totalColGap) / nCols;
        float cellHeight = (path.Bounds.Height - totalRowGap) / nRows;
        var combinedPath = new SKPath();
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nCols; col++)
            {
                float cellleft = path.Bounds.Left + col * (cellWidth + colGap);
                float celltop = path.Bounds.Top + row * (cellHeight + rowGap);

                var cellRect = new SKRect(
                    cellleft,
                    celltop,
                    cellleft + cellWidth,
                    celltop + cellHeight
                );

                using var cellPath = new SKPath();
                cellPath.AddRect(cellRect);
                using var clipped = new SKPath();
                path.Op(cellPath, SKPathOp.Intersect, clipped);

                if (!clipped.IsEmpty)
                {
                    // string cellGeometry = clipped.ToSvgPathData();
                    combinedPath.AddPath(clipped);
                }
            }
        }
        return geometryString;
    }

    /// <summary>
    /// Get boundingbox of selected object
    /// </summary>
    /// <returns></returns>
    public static Draw2D.Core.Geo.Rectangle GetBoundingBox(IEnumerable<Figure> figures)
    {
        if (figures == null || figures.Count() == 0)
            return  new Draw2D.Core.Geo.Rectangle(0,0,0,0);
        var minX = figures.Min(f => f.X);
        var minY = figures.Min(f => f.Y);
        var maxX = figures.Max(f => f.X + f.Width);
        var maxY = figures.Max(f => f.Y + f.Height);
        var boundingBox = new Draw2D.Core.Geo.Rectangle(minX, minY, maxX - minX, maxY - minY);
        return boundingBox;
    }
    public static List<Figure>? SplitLEDInToMatrix(Figure? figure, int nRows, int nCols, float colGap = 2, float rowGap = 2)
    {
        if (figure is not LEDContainerFigure)
            return null;


        if (figure == null ||
         figure is not LEDContainerFigure containerFigure ||
         containerFigure.ChildItem is not AmbinityLED led ||
          string.IsNullOrEmpty(led.Geometry))
            return null;

        var path = SKPath.ParseSvgPathData(led.Geometry);
        if (path == null)
            return null;

        //Transform Path to Figure size because path does not contain any size and position data
        var scaleX = containerFigure.Width / path.Bounds.Width;
        var scaleY = containerFigure.Height / path.Bounds.Height;
        var left = containerFigure.X;
        var top = containerFigure.Y;
        var toOrigin = SKMatrix.CreateTranslation(-path.Bounds.Left, -path.Bounds.Top);
        var translate = SKMatrix.CreateTranslation(left, top);
        var scale = SKMatrix.CreateScale(scaleX, scaleY);
        var temp = SKMatrix.Concat(toOrigin, translate);
        var finalMatrix = SKMatrix.Concat(temp, scale);
        path.Transform(finalMatrix);
        // Calculate total gap space
        float totalColGap = (nCols - 1) * colGap;
        float totalRowGap = (nRows - 1) * rowGap;
        // Calculate cell size with gaps
        float cellWidth = (path.Bounds.Width - totalColGap) / nCols;
        float cellHeight = (path.Bounds.Height - totalRowGap) / nRows;

        if (cellWidth < containerFigure.MinWidth || cellHeight < containerFigure.MinHeight)
            return null;
        var figures = new List<Figure>();
        for (int row = 0; row < nRows; row++)
        {
            for (int col = 0; col < nCols; col++)
            {
                float cellleft = path.Bounds.Left + col * (cellWidth + colGap);
                float celltop = path.Bounds.Top + row * (cellHeight + rowGap);

                var cellRect = new SKRect(
                    cellleft,
                    celltop,
                    cellleft + cellWidth,
                    celltop + cellHeight
                );

                using var cellPath = new SKPath();
                cellPath.AddRect(cellRect);
                using var clipped = new SKPath();
                path.Op(cellPath, SKPathOp.Intersect, clipped);

                if (!clipped.IsEmpty)
                {
                    string cellGeometry = clipped.ToSvgPathData();
                    var boundingBox = new Draw2D.Core.Geo.Rectangle(clipped.Bounds.Left, clipped.Bounds.Top, clipped.Bounds.Width, clipped.Bounds.Height);
                    var ledContainerFigure = CreateLED(cellGeometry, boundingBox);
                    if (ledContainerFigure != null)
                        figures.Add(ledContainerFigure);
                }
            }
        }
        return figures;
    }
}
