using System;
using System.Runtime.InteropServices;
using Serilog;

namespace Ambinity.SystemUtilities;

   public class SystemEventUtilities
    {

        private delegate void PowerCallback(IntPtr refCon, uint messageType, IntPtr messageArgument);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern IntPtr IORegisterForSystemPower(IntPtr refCon, IntPtr runLoop, PowerCallback callback, out IntPtr notifier);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern void IODeregisterForSystemPower(IntPtr notifier);

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        private static extern void CFRunLoopRun();

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        private static extern void CFRunLoopStop(IntPtr runLoop);

        private IntPtr _notifier;
        private PowerCallback _powerCallback;

        public event Action SleepEvent;
        public event Action WakeEvent;

        public void StartListening()
        {
            _powerCallback = PowerEventCallback;

            IntPtr runLoop = IntPtr.Zero; // Use the current run loop
            _notifier = IORegisterForSystemPower(IntPtr.Zero, runLoop, _powerCallback, out _notifier);

            if (_notifier == IntPtr.Zero)
            {
                throw new Exception("Failed to register for system power events.");
            }

            // Start the run loop to listen for events
            CFRunLoopRun();
        }

        public void StopListening()
        {
            if (_notifier != IntPtr.Zero)
            {
                IODeregisterForSystemPower(_notifier);
                _notifier = IntPtr.Zero;
            }

            // Stop the run loop
            CFRunLoopStop(IntPtr.Zero);
        }

        private void PowerEventCallback(IntPtr refCon, uint messageType, IntPtr messageArgument)
        {
            switch (messageType)
            {
                case 0x01: // Sleep event
                    SleepEvent?.Invoke();
                    break;
                case 0x02: // Wake event
                    WakeEvent?.Invoke();
                    break;
                default:
                    break;
            }
        }
    }
