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
using adrilight.ViewModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows;

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
        private string _hardwareVersion = "unknown";
        private string _productionDate;
        private bool _isVisible;
        private bool _isEnabled;
        private string _outputPort;
        private bool _isTransferActive;
        private bool _isDummy = false;
        private bool _isLoading = false;
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
        private bool _isLoadingProfile = false;
        private string _activatedProfileUID;
        private string _fwLocation;
        private State _currentState = State.normal;
        private string _requiredFwVersion;
        private static byte[] requestCommand = { (byte)'d', (byte)'i', (byte)'r' };
        private static byte[] expectedValidHeader = { 15, 12, 93 };


        public State CurrentState { get => _currentState; set { Set(() => CurrentState, ref _currentState, value); } }
        public string RequiredFwVersion { get => _requiredFwVersion; set { Set(() => RequiredFwVersion, ref _requiredFwVersion, value); } }
        public int DeviceID { get => _deviceID; set { Set(() => DeviceID, ref _deviceID, value); } }
        public string DeviceName { get => _deviceName; set { Set(() => DeviceName, ref _deviceName, value); } }
        public string FwLocation { get => _fwLocation; set { Set(() => FwLocation, ref _fwLocation, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => DeviceSerial, ref _deviceSerial, value); } }
        public string DeviceType { get => _deviceType; set { Set(() => DeviceType, ref _deviceType, value); } }
        public string Manufacturer { get => _manufacturer; set { Set(() => Manufacturer, ref _manufacturer, value); } }
        public string FirmwareVersion { get => _firmwareVersion; set { Set(() => FirmwareVersion, ref _firmwareVersion, value); } }
        public string HardwareVersion { get => _hardwareVersion; set { Set(() => HardwareVersion, ref _hardwareVersion, value); } }
        public string ProductionDate { get => _productionDate; set { Set(() => ProductionDate, ref _productionDate, value); } }
        public string ActivatedProfileUID { get => _activatedProfileUID; set { Set(() => ActivatedProfileUID, ref _activatedProfileUID, value); } }
        public bool IsVisible { get => _isVisible; set { Set(() => IsVisible, ref _isVisible, value); } }
        public bool IsEnabled { get => _isEnabled; set { Set(() => IsEnabled, ref _isEnabled, value); } }
        public bool IsUnionMode { get => _isUnionMode; set { Set(() => IsUnionMode, ref _isUnionMode, value); } }
        public bool IsLoading { get => _isLoading; set { Set(() => IsLoading, ref _isLoading, value); } }
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
        public bool IsLoadingProfile { get => _isLoadingProfile; set { Set(() => IsLoadingProfile, ref _isLoadingProfile, value); } }
        public void ActivateProfile(IDeviceProfile profile)
        {
            ActivatedProfileUID = profile.ProfileUID;
            for (var i = 0; i < AvailableOutputs.Length; i++)
            {
                AvailableOutputs[i].OutputIsLoadingProfile = true;

                foreach (PropertyInfo property in AvailableOutputs[i].GetType().GetProperties())
                {

                    if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
                        property.SetValue(AvailableOutputs[i], property.GetValue(profile.OutputSettings[i], null), null);
                }

                AvailableOutputs[i].OutputIsLoadingProfile = false;
            }
            if (profile.UnionOutput != null)
            {
                UnionOutput.OutputIsLoadingProfile = true;
                foreach (PropertyInfo property in UnionOutput.GetType().GetProperties())
                {
                    if (Attribute.IsDefined(property, typeof(ReflectableAttribute)))
                        property.SetValue(UnionOutput, property.GetValue(profile.UnionOutput, null), null);
                }
                UnionOutput.OutputIsLoadingProfile = false;
            }


        }


        public void RefreshFirmwareVersion()
        {

            byte[] id = new byte[256];
            byte[] name = new byte[256];
            byte[] fw = new byte[256];

            bool isValid = false;


            IsTransferActive = false; // stop current serial stream attached to this device

            var _serialPort = new SerialPort(OutputPort, 1000000);
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 1000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            //write request info command
            _serialPort.Write(requestCommand, 0, 3);
            int retryCount = 0;
            int offset = 0;
            int idLength = 0; // Expected response length of valid deviceID 
            int nameLength = 0; // Expected response length of valid deviceName 
            int fwLength = 0;
            IDeviceSettings newDevice = new DeviceSettings();
            while (offset < 3)
            {


                try
                {
                    byte header = (byte)_serialPort.ReadByte();
                    if (header == expectedValidHeader[offset])
                    {
                        offset++;
                    }
                }
                catch (TimeoutException)// retry until received valid header
                {
                    _serialPort.Write(requestCommand, 0, 3);
                    retryCount++;
                    if (retryCount == 3)
                    {
                        Console.WriteLine("timeout waiting for respond on serialport " + _serialPort.PortName);
                        HandyControl.Controls.MessageBox.Show("Device at " + _serialPort.PortName + "is not responding, try adding it manually", "Device is not responding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        isValid = false;
                        break;
                    }
                    Debug.WriteLine("no respond, retrying...");
                }


            }
            if (offset == 3) //3 bytes header are valid
            {
                idLength = (byte)_serialPort.ReadByte();
                int count = idLength;
                id = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(id, 0, count);
                    offset += readCount;
                    count -= readCount;
                }


                DeviceSerial = BitConverter.ToString(id).Replace('-', ' ');
                RaisePropertyChanged(nameof(DeviceSerial));
            }
            if (offset == 3 + idLength) //3 bytes header are valid
            {
                nameLength = (byte)_serialPort.ReadByte();
                int count = nameLength;
                name = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(name, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                DeviceName = Encoding.ASCII.GetString(name, 0, name.Length);
                RaisePropertyChanged(nameof(DeviceName));
                

            }
            if (offset == 3 + idLength + nameLength) //3 bytes header are valid
            {
                fwLength = (byte)_serialPort.ReadByte();
                int count = fwLength;
                fw = new byte[count];
                while (count > 0)
                {
                    var readCount = _serialPort.Read(fw, 0, count);
                    offset += readCount;
                    count -= readCount;
                }
                FirmwareVersion = Encoding.ASCII.GetString(fw, 0, fw.Length);
                RaisePropertyChanged(nameof(FirmwareVersion));
            }
            _serialPort.Close();
            _serialPort.Dispose();
            //if (isValid)
            //    newDevices.Add(newDevice);
            //reboot serialStream
            IsTransferActive = true;
            RaisePropertyChanged(nameof(IsTransferActive));
        }
    
}
}
