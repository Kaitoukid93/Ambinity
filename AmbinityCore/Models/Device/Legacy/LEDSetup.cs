using System.Collections.ObjectModel;
using System.Windows.Input;
using adrilight_shared.Models.Device.Zone.Spot;
using Avalonia;
using CommunityToolkit.Mvvm.Input;

namespace adrilight_shared.Models.Device.Zone;

/// <summary>
/// legacy adrilight ledsetup
/// </summary>
public class LEDSetup
{
    public LEDSetup()
    {
        Spots = new ObservableCollection<DeviceSpot>();
    }


    public string Name { get; set; }
    public string Description { get; set; }

    public string TargetType { get; set; } // Tartget Type of the spotset (keyboard, strips, ...
    public ObservableCollection<DeviceSpot> Spots { get; set; }
    public string Thumbnail { get; set; }
    private double _offsetX;
    private double _offsetY;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }

    public double Top { get; set; }

    public double Left { get; set; }
    public double Width { get; set; }

    public double Height { get; set; }
}