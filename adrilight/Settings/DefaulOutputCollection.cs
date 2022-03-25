using adrilight.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace adrilight.Settings
{
    internal class DefaulOutputCollection
    {
        public static OutputSettings AmbinoBasic(int id,int numLEDX, int numLEDY)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = "Ambino Basic",
                OutputID = id,
                OutputType = "Frame",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputLocationX = 0,
                OutputLocationY = 0,
                OutputPixelWidth = Screen.PrimaryScreen.Bounds.Width/8,
                OutputPixelHeight= Screen.PrimaryScreen.Bounds.Height/8,
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
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 10,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow)
            // create ledsetup if neccesary

        };
            return outputSettings;
            }

        public static OutputSettings AmbinoEdge(int id, int numLED)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = "Ambino EDGE",
                OutputID = id,
                OutputType = "Strip",
                OutputNumLED = 100,
                OutputNumLEDX = numLED,
                OutputNumLEDY = 1,
                OutputLocationX = 0,
                OutputLocationY = 0,
                OutputPixelWidth = Screen.PrimaryScreen.Bounds.Width / 8,
                OutputPixelHeight = Screen.PrimaryScreen.Bounds.Width/(8*numLED),
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
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 10,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow)
                // create ledsetup if neccesary

            };
            return outputSettings;
        }

        public static OutputSettings GenericLEDStrip(int id, int numLED)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = "Generic LED Strip",
                OutputID = id,
                OutputType = "Strip",
                OutputNumLED = 100,
                OutputNumLEDX = numLED,
                OutputNumLEDY = 1,
                OutputLocationX = 0,
                OutputLocationY = 0,
                OutputPixelWidth = Screen.PrimaryScreen.Bounds.Width / 8,
                OutputPixelHeight = Screen.PrimaryScreen.Bounds.Width / (8 * numLED),
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
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 10,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow)
                // create ledsetup if neccesary

            };
            return outputSettings;
        }
        public static OutputSettings GenericLEDMatrix(int id, int numLEDX,int numLEDY)
        {

            var outputSettings = new OutputSettings { //24 inch led frame for Ambino Basic
                OutputName = "Generic LED Strip",
                OutputID = id,
                OutputType = "Matrix",
                OutputNumLED = 100,
                OutputNumLEDX = numLEDX,
                OutputNumLEDY = numLEDY,
                OutputLocationX = 0,
                OutputLocationY = 0,
                OutputPixelWidth = Screen.PrimaryScreen.Bounds.Width / 8,
                OutputPixelHeight = Screen.PrimaryScreen.Bounds.Height/8,
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
                OutputSelectedAudioDevice = 0,
                OutputSelectedDisplay = 0,
                OutputSelectedChasingPalette = 0,
                OutputPaletteSpeed = 1,
                OutputPaletteBlendStep = 16,
                OutputStaticColor = Color.FromRgb(0, 255, 0),
                OutputBreathingSpeed = 10,
                OutputCurrentActivePalette = new ColorPalette("Full Rainbow", "Zooey", "RGBPalette16", "Full Color Spectrum", DefaultColorCollection.rainbow)
                // create ledsetup if neccesary

            };
            return outputSettings;
        }

    }
}

