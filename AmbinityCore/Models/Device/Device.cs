using AmbinityCore.Enums;

namespace AmbinityCore.Models.Device;

public class Device:IDevice
{
    public string Name { get; set; }
    public string Serial { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Manufacturer { get; set; }
    public string Description { get; set; }
    public string FirmwareVersion { get; set; }
    public string HardwareVersion { get; set; }
    public string ProductionDate { get; set; }
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public  string Address { get; set; }
    public  bool IsTransferActive { get; set; }
}