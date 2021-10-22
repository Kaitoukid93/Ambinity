using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Spots
{
    public interface IDeviceSpotSet
    {
        

        IDeviceSpot[] Spots { get; set; }
        
          
        object Lock { get; }
       
        int CountLeds(int spotsX, int spotsY);
        int ID { get; set; }
        int ParrentLocation { get; set; } // for Child in hub Object
        int OutputLocation { get; set; } // for Child in hub Object
        int RGBOrder { get; set; }
        string DeviceSerial { get;}
        string DeviceLocation { get; } //for OpenRGB Device
        void IndicateMissingValues();

    }
}
