using System.ComponentModel;
using Avalonia.Media;

namespace AmbinityCore.Models.Device;

public interface ILEDControllerHardwareSettings : INotifyPropertyChanged
{
    string DeviceSerialNumber { get; set; }
    string DeviceHardwareVersion { get; set; }
    string DeviceFirmwareVersion { get; set; }
    string DeviceManufacturer { get; set; }
    string DeviceCommunicationAddress { get; set; }
    string DeviceName { get; set; }

    HardwareTypeEnum HardwareType { get; set; }

    //Hardware lighting//
    bool HWL_enable { get; set; }
    bool StatusLEDEnable { get; set; }
    byte HWL_returnafter { get; set; }
    byte HWL_effectMode { get; set; }
    byte HWL_effectSpeed { get; set; }
    byte HWL_effectIntensity { get; set; }
    byte HWL_brightness { get; set; }
    Color HWL_singleColor { get; set; }
    Color[] HWL_palette { get; set; }
    byte HWL_version { get; set; }
    byte HWL_MaxLEDPerOutput { get; set; }
}