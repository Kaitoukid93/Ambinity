using adrilight.Spots;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight
{
    internal class LEDSettings : ViewModelBase, ILEDSettings
    {
        private int _deviceID = 1;
        private IDeviceSpot _spot0 = null;
        private string _deviceSerial = "151293";



        public int DeviceID { get => _deviceID; set { Set(() => DeviceID, ref _deviceID, value); } }
        public IDeviceSpot Spot0 { get => _spot0; set { Set(() => Spot0, ref _spot0, value); } }
        public string DeviceSerial { get => _deviceSerial; set { Set(() => DeviceSerial, ref _deviceSerial, value); } }

    }
}
