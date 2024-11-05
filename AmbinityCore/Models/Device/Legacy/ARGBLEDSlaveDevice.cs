using System.Collections.ObjectModel;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using AmbinityCore.Enums;

namespace adrilight_shared.Models.Device.SlaveDevice;

public class ARGBLEDSlaveDevice
{
    public ARGBLEDSlaveDevice()
    {
        ControlableZones = new ObservableCollection<LEDSetup>();
        
    }

    public List<DeviceSpot> GetSpots()
    {
        var spots = new List<DeviceSpot>();
        foreach (var zone in ControlableZones)
        {
            foreach (var spot in (zone as LEDSetup).Spots)
            {
                spots.Add(spot);
            }
        }

        return spots;
    }
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Vendor { get; set; }
    public int ParrentID { get; set; }

    public RGBLEDOrderEnum RGBLEDOrder { get; set; }
    public string Description { get; set; }
    public ObservableCollection<LEDSetup> ControlableZones { get; set; }
    public ImageVisual Image { get; set; }
    public int WhiteBalanceRed { get; set; }


    public int WhiteBalanceGreen { get; set; }

    public int WhiteBalanceBlue { get; set; }


    public double Top { get; set; }

    public double Left { get; set; }


    public double Width { get; set; }

    public double Height { get; set; }

    public double ActualWidth { get; set; }


    public double ActualHeight { get; set; }


    public double Scale { get; set; }


    public string Version { get; set; }
}