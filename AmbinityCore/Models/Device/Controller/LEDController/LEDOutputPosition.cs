using Avalonia;

namespace AmbinityCore.Models.Device;
/// <summary>
/// I don't know for what reason avalonia Rect refuse to deserialize so...
/// </summary>
public class LEDOutputPosition
{
    public LEDOutputPosition(double x, double y, double width, double height)
    {
        Left = x;
        Top = y;
        Width = width;
        Height = height;
    }

    public LEDOutputPosition()
    {
        
    }

    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public Rect ToRect()
    {
        return new Rect(Left, Top, Width, Height);
    }
}