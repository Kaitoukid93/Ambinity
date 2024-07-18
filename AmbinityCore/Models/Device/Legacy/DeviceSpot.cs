using Avalonia.Media;

namespace  adrilight_shared.Models.Device.Zone.Spot;

public class DeviceSpot
{
    public DeviceSpot()
    {
       
    }
    public int Index { get; set; } // Physical index
   
    public int MID { get; set; }
    public Geometry Geometry { get; set; } 
 
    public double Top { get; set; }

    public double Left { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
}