using System;
using Ambinity.Utils;

namespace Ambinity.Windows.SystemUtilities
{
    public static class SystemEvent
    {


        private static bool _isSubscribed = false;

        public static void Subscribe()
        {
            if (_isSubscribed)
                return;

            // Windows: Use SystemEvents.PowerModeChanged
            Microsoft.Win32.SystemEvents.PowerModeChanged += OnPowerModeChanged;
            _isSubscribed = true;
        }

        private static void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Suspend:
                     Utilities.Sleep();
                    break;
                case Microsoft.Win32.PowerModes.Resume:
                     Utilities.Wakeup();
                    break;
            }
        }
    }
}
