using Avalonia.Media;

namespace AmbinityCore.Models.Device.LED;

public class AmbinityLEDLayout
{
    public AmbinityLEDLayout(float x, float y, float width,float height,string geometry,int index)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Geometry = geometry;
        Index = index;
    }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Geometry { get; set; }
    public int Index { get; set; }
}