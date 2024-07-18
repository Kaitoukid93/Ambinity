using System.ComponentModel;
using AmbinityCore.Enums;

namespace AmbinityCore.Models.Device;

public interface ILEDController: INotifyPropertyChanged
{
    string DeviceName { get; set; }
    DeviceType DeviceType { get; set; }
    string DeviceDescription { get; set; }
    bool IsTransferEnabled { get; set; }
    ILEDControllerHardwareSettings IledControllerHardwareSettings { get; set; }
}