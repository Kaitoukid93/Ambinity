using AmbinityCore.Enums;

namespace AmbinityCore.Models.Device;

public interface IDevice
{
    string Name { get; set; }
    string Serial { get; set; }
    DeviceType DeviceType { get; set; }
    string Manufacturer { get; set; }
    string Description { get; set; }
    string FirmwareVersion { get; set; }
    string HardwareVersion { get; set; }
    string ProductionDate { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }
    string Address { get; set; }
    bool IsTransferActive { get; set; }
}