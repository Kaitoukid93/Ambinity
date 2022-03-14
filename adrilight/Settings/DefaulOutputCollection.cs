using adrilight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight.Settings
{
    internal class DefaulOutputCollection
    {
        public static OutputSettings rectangleFrameLEDSetupOutputType1 = new OutputSettings { //24 inch led frame for Ambino Basic
            OutputName = "LED Frame",
            OutputID = 0,
            OutputType = "Frame",
            OutputNumLED = 32,
            OutputNumLEDX = 11,
            OutputNumLEDY = 7,
            OutputLocationX = 0,
            OutputLocationY = 0,
            OutputPixelWidth = 240,
            OutputPixelHeight = 135,
            OutputUniqueID = "",
            OutputRGBLEDOrder = "GRB",
            OutputIsVisible = true,
            OutputBrightness = 80,
            OutputPowerVoltage = 5,
            OutputPowerMiliamps = 900,
            OutputSaturationThreshold = 10,
            OutputUseLinearLighting = false,
            OutputIsEnabled = true,
            OutputAtmosphereStartColor = Color.FromRgb(255, 0, 0),
            OutputAtmosphereStopColor = Color.FromRgb(255, 0, 0),
            OutputAtmosphereMode = "Dirrect",
            OutputSelectedMusicMode = 0,
            OutputSelectedMusicPalette = 0,
            OutputSelectedMode = 1,
            //OutputSentryModeColorSource 
            OutputSelectedAudioDevice = 0,
            OutputSelectedDisplay = 0,
            OutputSelectedChasingPalette = 0,
            OutputPaletteSpeed = 1,
            OutputPaletteBlendStep = 16,
            OutputStaticColor = Color.FromRgb(0, 255, 0),
            OutputIsBreathing = false,
            OutputBreathingSpeed = 10,
            OutputCurrentActivePalette = DefaultColorCollection.rainbow,
           // create ledsetup if neccesary



    };
    }
}

