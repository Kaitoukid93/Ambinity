using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string _firmwareVersion;
        private string _productionDate;
        private bool _isVisible;
        private bool _isEnabled;
        private string _outputPort;
        private bool _isTransferActive;
        private IOutputSettings[] _availableOutput;

        public int DeviceID { get =>_deviceID; set { Set(() => DeviceID, ref _deviceID, value); } }
        public string DeviceName { get => _deviceName; set { Set(() => Devicename, ref _deviceName, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => _deviceSerial, ref _deviceSerial, value); } }
        public string DeviceType { get => _deviceType; set { Set(() => _deviceType, ref _deviceType, value); } }
        public string Manufacturer { get => _manufacturer; set { Set(() => _manufacturer, ref _manufacturer, value); } }
        public string FirmwareVersion { get => _firmwareVersion; set { Set(() => FirmwareVersion, ref _firmwareVersion, value); } }
        public string ProductionDate { get => _productionDate; set { Set(() => ProductionDate, ref _productionDate, value); } }
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public string OutputPort { get => _outputPort; set { Set(() => OutputPort, ref _outputPort, value); } }
        public bool IsTransferActive { get => _isTransferActive; set { Set(() => IsTransferActive, ref _isTransferActive, value); } }
        public IOutputSettings[] AvailableOutputs { get => _availableOutput; set { Set(() => AvailableOutputs, ref _availableOutput, value); } }




    }
}
