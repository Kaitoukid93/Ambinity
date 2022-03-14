using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Controls;
using adrilight.Settings;
using adrilight.Util;
using HandyControl.Controls;

namespace adrilight.ViewModel
{
    public class AddDeviceViewModel : BaseViewModel
    {
        private IDeviceSettings _device;
        public IDeviceSettings Device {
            get { return _device; }
            set
            {
                if (_device == value) return;
                _device = value;
                RaisePropertyChanged();
            }
        }
        private IGroupSettings _group;
        public IGroupSettings Group {
            get { return _group; }
            set
            {
                if (_group == value) return;
                _group = value;
                RaisePropertyChanged();
            }
        }
        private int _stepIndex;

        public int StepIndex {
            get => _stepIndex;
#if NET40
            set => Set(nameof(StepIndex), ref _stepIndex, value);
#else
            set => Set(ref _stepIndex, value);
#endif
        }
        public RelayCommand<Panel> NextCmd => new RelayCommand<Panel>(Next);

        /// <summary>
        ///     上一步
        /// </summary>
        public RelayCommand<Panel> PrevCmd => new RelayCommand<Panel>(Prev);

        private void Next(Panel panel)
        {
            foreach (var stepBar in panel.Children.OfType<StepBar>())
            {
                stepBar.Next();
            }
        }

        private void Prev(Panel panel)
        {
            foreach (var stepBar in panel.Children.OfType<StepBar>())
            {
                stepBar.Prev();
            }
        }
        private IDesktopFrame DesktopFrame;
        public IList<String> _AvailableComPorts;
        public IList<String> AvailableComPorts {
            get
            {
                _AvailableComPorts = SerialPort.GetPortNames().Concat(new[] { "Không có" }).ToList();
                _AvailableComPorts.Remove("COM1");
                return _AvailableComPorts;
            }
        }
        private ObservableCollection<IDeviceSettings> _existedDevices;
        public ObservableCollection<IDeviceSettings> ExistedDevices {
            get { return _existedDevices; }
            set {  _existedDevices = value; }
        }
        private ObservableCollection<IGroupSettings> _existedGroup;
        public ObservableCollection<IGroupSettings> ExistedGroup {
            get { return _existedGroup; }
            set { _existedGroup = value; }
        }

        public AddDeviceViewModel(ObservableCollection<IDeviceSettings> devices, ObservableCollection<IGroupSettings> groups, IDesktopFrame desktopFrame)
        {
            ExistedDevices = devices;
            ExistedGroup = groups;
            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            ExistedDevicesName = new List<string>();
            foreach(var device in ExistedDevices)
            {
                if(!device.IsHUB)
                {
                    var name = device.DeviceName;
                    if (device.ParrentLocation != 151293)
                    {
                        var parrentName = "";
                        foreach (var parrent in ExistedDevices)
                            if (parrent.HUBID == device.ParrentLocation)
                                parrentName = parrent.DeviceName;
                        name = name + "(" + parrentName + ")";
                    }
                    //device is in hub


                    ExistedDevicesName.Add(name);
                }
              
            }
            

        }
       
      
        private bool _basicRev1Checked;
        public bool BasicRev1Checked {

            get { return _basicRev1Checked; }
            set
            {
                _basicRev1Checked = value;
                if (value)
                {
                    Device = DefaultDeviceCollection.ambinoBasic;
                }

            }
        }
        private bool _basicRev2Checked;
        public bool BasicRev2Checked {

            get { return _basicRev2Checked; }
            set
            {
                _basicRev2Checked = value;
                if (value)
                {
                    Device = DefaultDeviceCollection.ambinoBasic;

                }


            }
        }
        private bool _isNextable;
        public bool IsNextable {

            get { return _isNextable; }
            set
            {

                _isNextable = value;


            }
        }
        private bool _eDGEChecked;
        public bool EDGEChecked {

            get { return _eDGEChecked; }
            set
            {
                _eDGEChecked = value;
                if (value)
                {
                    AddedItemType = AvailableTypes.Device;
                    Device.DeviceType = "ABEDGE";
                    Device.DeviceLayout = 1;
                    Device.LayoutEnabled = false;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        
        private List<IDeviceSettings> _availableOutputs;
        public List<IDeviceSettings> AvailableOutputs {
            get { return _availableOutputs; }
            set {  _availableOutputs = value;}
        }
        public  enum AvailableTypes { Device,Group};
        private AvailableTypes _addedItemType;
        public AvailableTypes AddedItemType {
            get { return _addedItemType; }
            set { _addedItemType = value; }
        }
      


        private List<IDeviceSettings> _selectedOutputs;
        public List<IDeviceSettings> SelectedOutputs {
            get { return _selectedOutputs; }
            set { _selectedOutputs = value; }
        }
        private List<IDeviceSettings> _selectedChilds;
        public List<IDeviceSettings> SelectedChilds {
            get { return _selectedChilds; }
            set
            {
                _selectedChilds = value;
                SelectedChild_PropertyChanged();
            }
        }
        private void SelectedChild_PropertyChanged()
        {
            if (SelectedChilds != null)
            {
                foreach (var child in SelectedChilds)
                {
                    child.SelectedEffect = 1;
                    child.GroupID = Group.GroupID;
                    child.GroupLightingEnable = true;
                };
            }
        }
        //private List<string> _selectedOutputs;
        //public List<string> SelectedOutputs {
        //    get { return _selectedOutputs; }
        //    set { _selectedOutputs = value; }
        //}
        private List<string> _existedDevicesName;
        public List<string> ExistedDevicesName {
            get { return _existedDevicesName; }
            set { _existedDevicesName = value; }
        }
        private bool _fanHUBChecked;
        public bool FanHubChecked {

            get { return _fanHUBChecked; }
            set
            {
                _fanHUBChecked = value;
                if (value)
                {
                    AddedItemType = AvailableTypes.Device;
                    Device.DeviceType = "ABFANHUB"; // add parrent device
                    Device.DeviceID = ExistedDevices.Count() + 1;
                    Device.HUBID = ExistedDevices.Count() + 1;
                    Device.IsHUB = true;
                    AvailableOutputs = new List<IDeviceSettings>();//add child devices
                    for (int i = 1; i < 11; i++)
                    {
                        var fan = new DeviceSettings();
                        fan.DeviceName = "Fan" + i.ToString();
                        fan.IsVissible = false;
                        fan.ParrentLocation = Device.HUBID;
                        fan.SelectedEffect = 1;
                        fan.SelectedPalette = 0;
                        fan.DevicePowerVoltage = 5;
                        fan.DevicePowerMiliamps = 500;
                        fan.DeviceLayout = 0;
                        fan.OutputLocation = i-1;
                        fan.NumLED = 16;
                        fan.SpotsX = 5;
                        fan.SpotsY = 5;
                        fan.OffsetLed = 4;
                        fan.LayoutEnabled = false;
                        //fan.offset...
                        fan.DeviceID = Device.DeviceID + i;

                        AvailableOutputs.Add(fan);

                    }
                    //allow user to press 'Next'
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => AvailableOutputs);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _hUBV2Checked;
        public bool HUBV2Checked {

            get { return _hUBV2Checked; }
            set
            {
                _hUBV2Checked = value;
                if (value)
                {
                    AddedItemType = AvailableTypes.Device;
                    IsNextable = true;
                    Device.DeviceType = "ABHV2";
                    RaisePropertyChanged(() => Device.DeviceType);
                    RaisePropertyChanged(() => IsNextable);
                }
                
            }
        }
        private bool _groupChecked;
        public bool GroupChecked {

            get { return _groupChecked; }
            set
            {
                _groupChecked = value;
                if (value)
                {
                    AddedItemType = AvailableTypes.Group;
                    Group.GroupID = ExistedGroup.Count() + 1;
                    Group.GroupName = "Group1";
                    IsNextable = true;
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
      
        private bool _aRGB1Selected;
        public bool ARGB1Selected {

            get { return _aRGB1Selected; }
            set
            {
                _aRGB1Selected = value;
             

            }
        }
        private bool _aRGB2Selected;
        public bool ARGB2Selected {

            get { return _aRGB2Selected; }
            set
            {
                _aRGB2Selected = value;


            }
        }
        private bool _pCI1Selected;
        public bool PCI1Selected {

            get { return _pCI1Selected; }
            set
            {
                _pCI1Selected = value;


            }
        }
        private bool _pCI2Selected;
        public bool PCI2Selected {

            get { return _pCI2Selected; }
            set
            {
                _pCI2Selected = value;


            }
        }
        private bool _pCI3Selected;
        public bool PCI3Selected {

            get { return _pCI3Selected; }
            set
            {
                _pCI3Selected = value;


            }
        }
        private bool _pCI4Selected;
        public bool PCI4Selected {

            get { return _pCI4Selected; }
            set
            {
                _pCI4Selected = value;


            }
        }


        private bool _checked24inch;
        public bool Checked24inch{

            get { return _checked24inch; }
            set
            {
                _checked24inch = value;
                if (value)
                {
                    Device.SpotsX = 11;
                    Device.SpotsY = 7;
                    Device.OffsetLed=10;
                    Device.NumLED = 32;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _checked27inch;
        public bool Checked27inch {

            get { return _checked27inch; }
            set
            {
                _checked27inch = value;
                if (value)
                {
                    Device.SpotsX = 13;
                    Device.SpotsY = 7;
                    Device.OffsetLed = 12;
                    Device.NumLED = 36;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _checked29inch;
        public bool Checked29inch {

            get { return _checked29inch; }
            set
            {
                _checked29inch = value;
                if (value)
                {
                    Device.SpotsX = 14;
                    Device.SpotsY = 7;
                    Device.OffsetLed = 13;
                    Device.NumLED = 38;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _checked32inch;
        public bool Checked32inch {

            get { return _checked32inch; }
            set
            {
                _checked32inch = value;
                if (value)
                {
                    Device.SpotsX = 14;
                    Device.SpotsY = 9;
                    Device.OffsetLed = 13;
                    Device.NumLED = 42;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _checked34inch;
        public bool Checked34inch {

            get { return _checked34inch; }
            set
            {
                _checked34inch = value;
                if (value)
                {
                    Device.SpotsX = 16;
                    Device.SpotsY = 7;
                    Device.OffsetLed = 15;
                    Device.NumLED = 42;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }
        private bool _checked1m2;
        public bool Checked1m2 {

            get { return _checked1m2; }
            set
            {
                _checked1m2 = value;
                if (value)
                {
                    Device.SpotsX = 48;
                    Device.SpotsY = 1;
                    Device.NumLED = 48;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }

        private bool _checked2m;
        public bool Checked2m {

            get { return _checked2m; }
            set
            {
                _checked2m = value;
                if (value)
                {
                    Device.SpotsX = 80;
                    Device.SpotsY = 1;
                    Device.NumLED = 80;
                    IsNextable = true;
                    RaisePropertyChanged(() => Device.SpotsX);
                    RaisePropertyChanged(() => Device.NumLED);
                    RaisePropertyChanged(() => Device.SpotsY);
                    RaisePropertyChanged(() => IsNextable);
                }

            }
        }

        public ObservableCollection<string> AvailableDevice { get; private set; }

        /// <summary>
        /// ReadData
        /// </summary>
        public override void ReadData()
        {
            Device = new DeviceSettings();
            Group = new GroupSettings();
            AvailableDevice = new ObservableCollection<string>
{
          "Ambino Basic Rev1",
           "Ambino Basic Rev2",
           "Ambino EDGE",
           "Ambino HUBV2",
          "Custom Device"


        };

            //_allDeviceView = new AllNewDeviceViewModel(this);
            //CurrentView = _allDeviceView;
        }


        //public void GoAllDeviceView()
        //{
        //    _allDeviceView = new AllNewDeviceViewModel(this);
        //    CurrentView = _allDeviceView;
        //}
        //public void GoToChangeNameView(IDeviceSettings device)
        //{
        //    Device = device;
        //    _changeNameView = new ChangeDeviceNameViewModel(this, Device);
        //    CurrentView = _changeNameView;
        //}
        //public void GoToChangePort(IDeviceSettings device)
        //{
        //    Device = device;
        //    _changePortView = new ChangePortViewModel(this, device);
        //    CurrentView = _changePortView;
        //}

    }
}
