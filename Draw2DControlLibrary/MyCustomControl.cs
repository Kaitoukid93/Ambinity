using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Draw2DControlLibrary;

public class MyCustomControl : Control
{
    public IBrush? Background { get; set; }

    public sealed override void Render(DrawingContext context)
    {
        if (Background != null)
        {
            var renderSize = Bounds.Size;
            context.FillRectangle(Background, new Rect(renderSize));
        }
            
        base.Render(context);
    }
}