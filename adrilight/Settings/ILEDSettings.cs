using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight
{
    public interface ILEDSettings
    {
        int DeviceID { get; set; }
        string DeviceSerial { get; set; }
        IDeviceSpot Spot0 { get; set; }

       

    }
}
