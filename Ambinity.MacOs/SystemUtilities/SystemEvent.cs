using System;
using Ambinity.Utils;
using AppKit;
using Foundation;

namespace Ambinity.MacOs.SystemUtilities
{
    public static class SystemEvent
    {

        private static bool _isSubscribed = false;

        public static void Subscribe()
        {
            if (_isSubscribed)
                return;
            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                NSWorkspace.WillSleepNotification,
                OnSystemSleep);

            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                NSWorkspace.DidWakeNotification,
                OnSystemWakeup);

            _isSubscribed = true;
        }

        private static void OnSystemSleep(NSNotification notification)
        {
            Utilities.Sleep();
        }

        private static void OnSystemWakeup(NSNotification notification)
        {
            Utilities.Wakeup();
        }
    }
}
