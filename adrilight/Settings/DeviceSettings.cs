using adrilight.Settings;
using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class DeviceSettings : ViewModelBase, IDeviceSettings
    {
        private int _deviceID;
        private string _deviceName;
        private string _deviceSerial;
        private string _deviceType;
        private string _manufacturer;
        private string _deviceDescription;
        private string _firmwareVersion;
        private string _productionDate;
        private bool _isVisible;
        private bool _isEnabled;
        private string _outputPort;
        private bool _isTransferActive;
        private bool _isDummy = false;
        private IOutputSettings[] _availableOutput;
        private IOutputSettings _unionOutput;
        private string _groupName = "Ambino Devices";
        private string _smallIcon = "";
        private string _bigIcon = "";
        private int _selectedOutput = 0;
        private string _geometry = "generaldevice";
        private string _deviceConnectionGeometry = "connection";
        private int _baudrate = 1000000;
        private int _activatedProfileIndex = 0;
        private string _deviceUID;
        private string _deviceConnectionType = "wired";
        private bool _isSelected = false;
        private bool _isUnionMode = false;
        




        public int DeviceID { get =>_deviceID; set { Set(() => DeviceID, ref _deviceID, value); } }
        public string DeviceName { get => _deviceName; set { Set(() => DeviceName, ref _deviceName, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => _deviceSerial, ref _deviceSerial, value); } }
        public string DeviceType { get => _deviceType; set { Set(() => _deviceType, ref _deviceType, value); } }
        public string Manufacturer { get => _manufacturer; set { Set(() => _manufacturer, ref _manufacturer, value); } }
        public string FirmwareVersion { get => _firmwareVersion; set { Set(() => FirmwareVersion, ref _firmwareVersion, value); } }
        public string ProductionDate { get => _productionDate; set { Set(() => ProductionDate, ref _productionDate, value); } }
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public bool IsUnionMode { get => _isUnionMode; set { Set(() => IsUnionMode, ref _isUnionMode, value); } }
        public bool IsSelected { get => _isSelected; set { Set(() => IsSelected, ref _isSelected, value); } }
        public string OutputPort { get => _outputPort; set { Set(() => OutputPort, ref _outputPort, value); } }
        public bool IsTransferActive { get => _isTransferActive; set { Set(() => IsTransferActive, ref _isTransferActive, value); } }
        public bool IsDummy { get => _isDummy; set { Set(() => IsDummy, ref _isDummy, value); } }
        public IOutputSettings[] AvailableOutputs { get => _availableOutput; set { Set(() => AvailableOutputs, ref _availableOutput, value); } }
        public IOutputSettings UnionOutput { get => _unionOutput; set { Set(() => UnionOutput, ref _unionOutput, value); } }
        public int Baudrate { get => _baudrate; set { Set(() => Baudrate, ref _baudrate, value); } }
        public int ActivatedProfileIndex { get => _activatedProfileIndex; set { Set(() => ActivatedProfileIndex, ref _activatedProfileIndex, value); } }
        public string GroupName { get => _groupName; set { Set(() => GroupName, ref _groupName, value); } }
        public string SmallIcon { get => _smallIcon; set { Set(() => SmallIcon, ref _smallIcon, value); } }
        public string BigIcon { get => _bigIcon; set { Set(() => BigIcon, ref _bigIcon, value); } }
        public int SelectedOutput { get => _selectedOutput; set { Set(() => SelectedOutput, ref _selectedOutput, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public string DeviceDescription { get => _deviceDescription; set { Set(() => DeviceDescription, ref _deviceDescription, value); } }
        public string DeviceUID { get => _deviceUID; set { Set(() => DeviceUID, ref _deviceUID, value); } }
        public string DeviceConnectionGeometry { get => _deviceConnectionGeometry; set { Set(() => DeviceConnectionGeometry, ref _deviceConnectionGeometry, value); } }
        public string DeviceConnectionType { get => _deviceConnectionType; set { Set(() => DeviceConnectionType, ref _deviceConnectionType, value); } }
        public void ActivateProfile(IDeviceProfile profile) 
        {
            for (var i = 0; i < AvailableOutputs.Length; i++)
            {

                foreach (PropertyInfo property in typeof(IOutputSettings).GetProperties().Where(p => p.CanWrite))
                {
                    property.SetValue(AvailableOutputs[i], property.GetValue(profile.OutputSettings[i], null), null);
                }
                //foreach (PropertyInfo property in CurrentDevice.AvailableOutputs[i].GetType().GetProperties())
                //{
                //property.SetValue(property);
                //    // do something with the property
                //}

            }
            if(profile.UnionOutput!=null)
            {
                foreach (PropertyInfo property in typeof(IOutputSettings).GetProperties().Where(p => p.CanWrite))
                {
                    property.SetValue(UnionOutput, property.GetValue(profile.UnionOutput, null), null);
                }
            }
            
        }
    }
}
