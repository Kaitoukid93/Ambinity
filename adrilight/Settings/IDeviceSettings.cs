using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;
using adrilight.Settings;

namespace adrilight
{
    public interface IDeviceSettings : INotifyPropertyChanged
    {
      
        int DeviceID { get; set; }
        string DeviceName { get; set; }
        string DeviceSerial { get; set; }
        string DeviceType { get; set; }
        string Manufacturer { get; set; }
        string DeviceDescription { get; set; }
        string FirmwareVersion { get; set; }
        string ProductionDate { get; set; }
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        bool IsDummy { get; set; }
        string OutputPort { get; set; }
        bool IsTransferActive { get; set; }
        int ActivatedProfileIndex { get; set; }
        IOutputSettings[] AvailableOutputs { get; set; }
        IOutputSettings UnionOutput { get; set; }
        string GroupName { get; set; }
        int SelectedOutput { get; set; }
        string Geometry { get; set; }
        string DeviceConnectionGeometry { get; set; }
        int Baudrate { get; set; }
        string DeviceUID { get; set; }
        string DeviceConnectionType { get; set; }
        bool IsSelected { get; set; }
        bool IsUnionMode { get; set; }
        void ActivateProfile(IDeviceProfile profile);
       
    }
}
