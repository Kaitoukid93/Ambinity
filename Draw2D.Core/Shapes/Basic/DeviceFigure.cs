namespace Draw2D.Core.Shapes.Basic;

public class DeviceFigure : Rectangle
{
    public DeviceFigure(float x, float y, float width, float height) : base(x, y, width,
        height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        SnapTargets = SnapTargets.Center | SnapTargets.MidPoints | SnapTargets.Vertices;
    }
}