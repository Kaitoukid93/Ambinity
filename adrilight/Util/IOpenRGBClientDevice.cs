﻿using OpenRGB.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRGB.NET;

namespace adrilight.Util
{
   public interface IOpenRGBClientDevice
    {
        Device[] DeviceList { get; set; }
        OpenRGBClient AmbinityClient { get; set; }
        void RefreshOpenRGBDeviceState();
        bool IsAvailable { get;set; }
    }
}
