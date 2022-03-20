using adrilight.Spots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using adrilight.Util;

namespace adrilight
{
    public interface IOutputSettings : INotifyPropertyChanged
    {
        //bool Autostart { get; set; }
     
        string OutputName { get; set; }
        int OutputID { get; set; }
        string OutputType { get; set; }
        int OutputNumLED { get; set; }
        int OutputNumLEDX { get; set; }
        int OutputNumLEDY { get; set; }
        int OutputLocationX { get; set; }
        int OutputLocationY { get; set; }
        int OutputPixelWidth { get; set; }
        int OutputPixelHeight { get; set; }
        string OutputUniqueID { get; set; }
        string OutputRGBLEDOrder { get; set; }
        bool OutputIsVisible { get; set; }
        int OutputBrightness { get; set; }
        int OutputPowerVoltage { get; set; }
        int OutputPowerMiliamps { get; set; }
        byte OutputSaturationThreshold { get; set; }

        bool OutputUseLinearLighting { get; set; }
        bool OutputIsEnabled { get; set; }
        Color OutputAtmosphereStartColor { get; set; }
        Color OutputAtmosphereStopColor { get; set; }
        string OutputAtmosphereMode { get; set; }
        //string SelectedEffect { get; set; }
        int OutputSelectedMusicMode { get; set; }
        int OutputSelectedMusicPalette { get; set; }
        Color[] OutputSentryModeColorSource { get; set; }
        int OutputSelectedAudioDevice { get; set; }
        int OutputSelectedDisplay { get; set; }
        int OutputSelectedMode { get; set; }
        bool IsInSpotEditWizard { get; set; } 





        //rainbow settings//
        int OutputSelectedChasingPalette { get; set; }
        int OutputPaletteSpeed { get; set; }
        int OutputPaletteBlendStep { get; set; } // auto adjust step based on numLED
        //rainbow settings//

        //static color settings//
        Color OutputStaticColor { get; set; }
        bool OutputIsBreathing { get; set; }
        int OutputBreathingSpeed { get; set; }
        //static color settings//


 
        Color[] OutputCurrentActivePalette { get; set; }
        ILEDSetup OutputLEDSetup { get; set; }
        

    }
}
