using Avalonia;

namespace Ambinity.Views;

public static class VisualTransformHelper
{
    public static Rect TransformBoundsTo(Visual visual, Visual relativeTo)
    {
        if (visual.TransformToVisual(relativeTo) is Matrix trasformation)
        {
            var p0 = trasformation.Transform(default);
            var p1 = trasformation.Transform(new(visual.Bounds.Width, visual.Bounds.Height));
            return new(p0, p1);
        }

        return default;
    }

}