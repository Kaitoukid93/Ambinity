﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    public interface IGeneralSettings : INotifyPropertyChanged
    {

       
        bool Autostart { get; set; }
        bool NotificationEnabled { get; set; }
        int SelectedAudioDevice { get; set; }
        
        bool IsOpenRGBEnabled { get; set; }    
        bool IsProfileLoading { get; set; }
        bool StartMinimized { get; set; }
        int SystemRainbowSpeed { get; set; }
        int SystemRainbowMaxTick { get; set; }


    }
}
