namespace AmbinityCore.Models.Lighting.Zone.Configuration;

/// <summary>
/// Represent capture area in ratio
/// </summary>
public class CaptureArea
{
    public CaptureArea(double ratioX, double ratioY, double ratioWidth, double ratioHeight)
    {
        RatioX = ratioX;
        RatioY = ratioY;
        RatioWidth = ratioWidth;
        RatioHeight = ratioHeight;
    }

    public double RatioX { get; set; }
    public double RatioY { get; set; }
    public double RatioWidth { get; set; }
    public double RatioHeight { get; set; }

    public override string ToString()
    {
        return "[" + RatioX.ToString() + "-" + RatioY.ToString() + "-" + RatioWidth.ToString() + "-" +
               RatioHeight.ToString() + "]";
    }
}