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

        int BorderDistanceX { get; set; }
        int BorderDistanceY { get; set; }
        bool Autostart { get; set; }
        bool MirrorX { get; set; }
        bool IsOpenRGBEnabled { get; set; }

        bool MirrorY { get; set; }

        int OffsetLed { get; set; }
        int OffsetLed2 { get; set; }
        int OffsetLed3 { get; set; }
        string SelectedShader { get; set; }

        int SpotHeight { get; set; }

        int SpotWidth { get; set; }

        int SpotsX { get; set; }
        int SpotsX2 { get; set; }
        int SpotsX3 { get; set; }

        int SpotsY { get; set; }
        int SpotsY2 { get; set; }
        int SpotsY3 { get; set; }
        int ShaderX { get; set; }
        int ShaderCanvasWidth { get; set; }
        int ShaderCanvasHeight { get; set; }
        int ShaderY { get; set; }
        int SentryMode { get; set; }
        int ScreenSize { get; set; }
        int DeskSize { get; set; }
        byte WhitebalanceRed { get; set; }
        byte WhitebalanceGreen { get; set; }
        byte WhitebalanceBlue { get; set; }
        int ScreenSizeSecondary { get; set; }
        int ScreenSizeThird { get; set; }
        //smooth choice
        int SmoothFactor { get; set; }
        int SelectedDisplay { get; set; }
        int SelectedAdapter { get; set; }
        byte SaturationTreshold { get; set; }
        bool ShouldbeRunning { get; set; }
        bool ShouldbeRunningSecondary { get; set; }
        bool ShouldbeRunningThird { get; set; }
        int UseLinearLighting { get; set; }
        int LimitFps { get; set; }
        bool StartMinimized { get; set; }


    }
}
