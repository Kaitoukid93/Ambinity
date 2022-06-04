using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class GeneralSettings : ViewModelBase, IGeneralSettings
    {
        // private bool _autostart = true;
        private bool _isOpenRGBEnabled = false;
        private bool _autostart = true;
        private int _systemRainbowSpeed = 5;
        private int _systemRainbowMaxTick = 1024;
        private bool _isProfileLoading = false;
        private bool _startMinimized = false;
        private bool _notificationEnabled = true;
        
     

  
        public int SystemRainbowMaxTick { get => _systemRainbowMaxTick; set { Set(() => SystemRainbowMaxTick, ref _systemRainbowMaxTick, value); } }
        private int _limitFps = 100;
        public bool StartMinimized { get => _startMinimized; set { Set(() => StartMinimized, ref _startMinimized, value); } }
        public bool Autostart { get => _autostart; set { Set(() => Autostart, ref _autostart, value); } }
        public bool IsOpenRGBEnabled { get => _isOpenRGBEnabled; set { Set(() => IsOpenRGBEnabled, ref _isOpenRGBEnabled, value); } }
        public bool NotificationEnabled { get => _notificationEnabled; set { Set(() => NotificationEnabled, ref _notificationEnabled, value); } }
        public int LimitFps { get => _limitFps; set { Set(() => LimitFps, ref _limitFps, value); }  }
        public int SystemRainbowSpeed { get => _systemRainbowSpeed; set { Set(() => SystemRainbowSpeed, ref _systemRainbowSpeed, value); } }
        public bool IsProfileLoading { get => _isProfileLoading; set { Set(() => IsProfileLoading, ref _isProfileLoading, value); } }
    }
}
