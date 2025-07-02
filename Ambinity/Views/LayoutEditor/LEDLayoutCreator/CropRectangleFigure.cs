using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;
/// <summary>
/// This inherited from basic Rectangle but render differently
/// </summary>ledfigure
public class CropRectangleFigure : Rectangle
{
    public bool KeepAspectRatio = true;
    public CropRectangleFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {

    }
    /// <summary>
    /// because crop rectangle always keep ratio, we need to override resize method
    /// </summary>
    /// <param name="dTop"></param>
    /// <param name="dRight"></param>
    /// <param name="dBottom"></param>
    /// <param name="dLeft"></param>
    public override void Resize(float dTop, float dRight, float dBottom, float dLeft)
    {

        // if (!KeepAspectRatio)
        // {
            base.Resize(dTop, dRight, dBottom, dLeft);
        // }
        // else
        // {
        //     var backup = GetAbsoluteBounds();
        //     var currentRatio = base.Width / base.Height;
        //     base.Resize(dTop, dRight, dBottom, dLeft);
        //     if (base.X + base.Width > Canvas.Width || base.Y + base.Width / currentRatio > Canvas.Height)
        //     {
        //         ForceSetDimensions(backup);
        //         return;
        //     }

        //     ForceSetDimensions(new Draw2D.Core.Geo.Rectangle(base.X, base.Y, base.Width, base.Width / currentRatio));
        // }

    }
    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
         var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
            var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);
       var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
            var thickness = StrokeThickness;
            if (OverrideStrokeStyle)
            {
                strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
                thickness = (float)strokeThickness;
            }


        var pen = new Pen(strokeBrush, thickness)
        {
            DashStyle = DashStyle
        };
        //only render crop area transparrent
        ImmutableSolidColorBrush solidColorBrush2 = new ImmutableSolidColorBrush(Colors.Transparent);

        //get the geometry path created by the image and the crop rectangle, the corordinateSystem is fixed (from bottom to top and left to right)
        // I'm too lazy to edit the library and compile again so we add an offset to cropRectGeometry
        var cropRectGeometry = new RectangleGeometry(new Rect(base.X, base.Y, base.Width, base.Height));
        var imageRectGeometry = new RectangleGeometry(new Rect(0, 0, Canvas.Width, Canvas.Height));
        CombinedGeometry combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, imageRectGeometry, cropRectGeometry);

        dc.DrawGeometry(new SolidColorBrush(Colors.Black.AdjustOpacity(0.7)), null, combinedGeometry);
         Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
            dc.PushTransform(translate);
        dc.DrawRectangle(solidColorBrush2, pen, new Rect(new Point(base.X, base.Y), new Size(base.Width, base.Height)));
    }
}
