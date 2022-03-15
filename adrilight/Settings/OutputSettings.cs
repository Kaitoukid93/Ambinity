using adrilight.Spots;
using adrilight.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace adrilight
{
    internal class OutputSettings : ViewModelBase, IOutputSettings
    {
       
        private string _outputName;
        private int _outputID;
        private int _outputNumLED;
        private int _outputNumLEDX;
        private int _outputNumLEDY;
        private int _outputLocationX;
        private int _outputLocationY;
        private int _outputPixelWidth;
        private int _outputPixelHeight;
        private string _outputUniqueID;
        private string _outputRGBLEDOrder;
        private bool _outputIsVisible;
        private int _outputBrightness;
        private int _outputPowerVoltage;
        private int _outputPowerMiliamps;
        private byte _outputSaturationThreshold;
        private bool _outputUseLinearLighting;
        private bool _outputIsEnable;
        private Color _outputAtmosphereStartColor;
        private Color _outputAtmosphereStopColor;
        private string _outputAtmosphereMode;
        private int _outputSelectedMusicMode;
        private int _outputSelectedMusicPalette;
        private Color[] _outputSentryModeColorSource;
        private int _outputSelectedAudioDevice;
        private int _outputSelectedDisplay;
        private int _outputSelectedChasingPalette;
        private int _outputSelectedMode;
        private int _outputPaletteSpeed;
        private int _outputPaletteBlendStep;
        private Color _outputStaticColor;
        private bool _outputIsBreathing;
        private int _outputBreathingSpeed;
        private Color[] _outputCurrentActivePalette;
        private ILEDSetup _outputLEDSetup;

        public string OutputName  { get => _outputName; set { Set(() => OutputName, ref _outputName, value);}}
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        public string OutputType { get => _outputName; set { Set(() => OutputType, ref _outputName, value); } }
        public int OutputNumLED { get => _outputNumLED; set { Set(() => OutputNumLED, ref _outputNumLED, value); } }
        public int OutputNumLEDX { get => _outputNumLEDX; set { Set(() => OutputNumLEDX, ref _outputNumLEDX, value); } }
        public int OutputNumLEDY { get => _outputNumLEDY; set { Set(() => OutputNumLEDY, ref _outputNumLEDY, value); } }
        public int OutputLocationX { get => _outputLocationX; set { Set(() => OutputLocationX, ref _outputLocationX, value); } }
        public int OutputLocationY { get => _outputLocationY; set { Set(() => OutputLocationY, ref _outputLocationY, value); } }
        public int OutputPixelWidth { get => _outputPixelWidth; set { Set(() => OutputPixelWidth, ref _outputPixelWidth, value); } }
        public int OutputPixelHeight { get => _outputPixelHeight; set { Set(() => OutputPixelHeight, ref _outputPixelHeight, value); } }
        public string OutputUniqueID { get => _outputUniqueID; set { Set(() => OutputUniqueID, ref _outputUniqueID, value); } }
        public string OutputRGBLEDOrder { get => _outputRGBLEDOrder; set { Set(() => OutputRGBLEDOrder, ref _outputRGBLEDOrder, value); } }
        public bool OutputIsVisible { get => _outputIsVisible; set { Set(() => OutputIsVisible, ref _outputIsVisible, value); } }
        public int OutputBrightness { get => _outputBrightness; set { Set(() => OutputBrightness, ref _outputBrightness, value); } }
        public int OutputPowerVoltage { get => _outputPowerVoltage; set { Set(() => OutputPowerVoltage, ref _outputPowerVoltage, value); } }
        public int OutputPowerMiliamps { get => _outputPowerMiliamps; set { Set(() => OutputPowerMiliamps, ref _outputPowerMiliamps, value); } }
        public byte OutputSaturationThreshold { get => _outputSaturationThreshold; set { Set(() => OutputSaturationThreshold, ref _outputSaturationThreshold, value); } }
        public bool OutputUseLinearLighting { get => _outputUseLinearLighting; set { Set(() => OutputUseLinearLighting, ref _outputUseLinearLighting, value); } }
        public bool OutputIsEnabled { get => _outputIsEnable; set { Set(() => OutputIsEnabled, ref _outputIsEnable, value); } }
        public Color OutputAtmosphereStartColor { get => _outputAtmosphereStartColor; set { Set(() => OutputAtmosphereStartColor, ref _outputAtmosphereStartColor, value); } }
        public Color OutputAtmosphereStopColor { get => _outputAtmosphereStopColor; set { Set(() => OutputAtmosphereStopColor, ref _outputAtmosphereStopColor, value); } }
        public string OutputAtmosphereMode { get => _outputAtmosphereMode; set { Set(() => OutputAtmosphereMode, ref _outputAtmosphereMode, value); } }
        //string SelectedEffect { get; set; }
        public int OutputSelectedMusicMode { get => _outputSelectedMusicMode; set { Set(() => OutputSelectedMusicMode, ref _outputSelectedMusicMode, value); } }
        public int OutputSelectedMode { get => _outputSelectedMode; set { Set(() => OutputSelectedMode, ref _outputSelectedMode, value); } }
        public int OutputSelectedMusicPalette { get => _outputSelectedMusicPalette; set { Set(() => OutputSelectedMusicPalette, ref _outputSelectedMusicPalette, value); } }
        public Color[] OutputSentryModeColorSource { get => _outputSentryModeColorSource; set { Set(() => OutputSentryModeColorSource, ref _outputSentryModeColorSource, value); } }
        public int OutputSelectedAudioDevice { get => _outputSelectedAudioDevice; set { Set(() => OutputSelectedAudioDevice, ref _outputSelectedAudioDevice, value); } }
        public int OutputSelectedDisplay { get => _outputSelectedDisplay; set { Set(() => OutputSelectedDisplay, ref _outputSelectedDisplay, value); } }
        public int OutputSelectedChasingPalette { get => _outputSelectedChasingPalette; set { Set(() => OutputSelectedChasingPalette, ref _outputSelectedChasingPalette, value); } }
        public int OutputPaletteSpeed { get => _outputPaletteSpeed; set { Set(() => OutputPaletteSpeed, ref _outputPaletteSpeed, value); } }
        public int OutputPaletteBlendStep { get => _outputPaletteBlendStep; set { Set(() => OutputPaletteBlendStep, ref _outputPaletteBlendStep, value); } }
        public Color OutputStaticColor { get => _outputStaticColor; set { Set(() => OutputStaticColor, ref _outputStaticColor, value); } }
        public bool OutputIsBreathing { get => _outputIsBreathing; set { Set(() => OutputIsBreathing, ref _outputIsBreathing, value); } }
        public int OutputBreathingSpeed { get => _outputBreathingSpeed; set { Set(() => OutputBreathingSpeed, ref _outputBreathingSpeed, value); } }
        public Color[] OutputCurrentActivePalette { get => _outputCurrentActivePalette; set { Set(() => OutputCurrentActivePalette, ref _outputCurrentActivePalette, value); } }
        public ILEDSetup OutputLEDSetup { get => _outputLEDSetup; set { Set(() => OutputLEDSetup, ref _outputLEDSetup, value); } }

    }
}
