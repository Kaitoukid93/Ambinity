using System.ComponentModel;

namespace AmbinityCore.Models.Device;

public interface ILEDControllerHardwareSettings : INotifyPropertyChanged
{
    string DeviceSerialNumber { get; set; }
    string DeviceHardwareVersion { get; set; }
    string DeviceFirmwareVersion { get; set; }
    string DeviceManufacturer { get; set; }
    string DeviceCommunicationAddress { get; set; }
}