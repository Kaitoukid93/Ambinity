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
        private string _outputType;
        private int _outputID;
        private int _outputNumLED;
        private int _outputNumLEDX;
        private int _outputNumLEDY;
    

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
        private int _outputStaticColorMode=0;
        private int _outputStaticColorGradientMode = 0;
        private int _outputScreenCapturePosition = 0;
        private int _outputScreenCaptureWB = 0;
        private int _outputMusicDancingMode = 0;
        private int _outputColorPaletteMode = 0;
        private int _outputBreathingSpeed;
        private IColorPalette _outputCurrentActivePalette;
        private ILEDSetup _outputLEDSetup;
        private bool _isInSpotEditWizard=false;
        private string _geometry = "generaldevice";
        private int _outputSmoothness = 2;
        private int _outputPaletteChasingPosition;
        private int _outputScreenCaptureWBRed=100;
        private int _outputScreenCaptureWBGreen=100;
        private int _outputScreenCaptureWBBlue=100;
        private int _outputScreenCapturePositionIndex = 0;
        private System.Drawing.Rectangle _outputRectangle;
        private bool _outputIsLoadingProfile = false;
        private bool _outputIsBuildingLEDSetup = false;
        private int _outputMusicSensitivity = 10;
        private int _outputMusicVisualizerFreq = 0;



        public string OutputName  { get => _outputName; set { Set(() => OutputName, ref _outputName, value);}}
        public int OutputID { get => _outputID; set { Set(() => OutputID, ref _outputID, value); } }
        public string OutputType { get => _outputType; set { Set(() => OutputType, ref _outputType, value); } }
        public int OutputNumLED { get => _outputNumLED; set { Set(() => OutputNumLED, ref _outputNumLED, value); } }
        public int OutputNumLEDX { get => _outputNumLEDX; set { Set(() => OutputNumLEDX, ref _outputNumLEDX, value); } }
        public int OutputNumLEDY { get => _outputNumLEDY; set { Set(() => OutputNumLEDY, ref _outputNumLEDY, value); } }
   
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
        public int OutputStaticColorMode { get => _outputStaticColorMode; set { Set(() => OutputStaticColorMode, ref _outputStaticColorMode, value); } }
        public int OutputColorPaletteMode { get => _outputColorPaletteMode; set { Set(() => OutputColorPaletteMode, ref _outputColorPaletteMode, value); } }
        public int OutputStaticColorGradientMode { get =>_outputStaticColorGradientMode; set { Set(() => OutputStaticColorGradientMode, ref _outputStaticColorGradientMode, value); } }
        public int OutputScreenCapturePosition { get => _outputScreenCapturePosition; set { Set(() => OutputScreenCapturePosition, ref _outputScreenCapturePosition, value); } }
        public int OutputScreenCaptureWB { get => _outputScreenCaptureWB; set { Set(() => OutputScreenCaptureWB, ref _outputScreenCaptureWB, value); } }
        public int OutputMusicDancingMode { get => _outputMusicDancingMode; set { Set(() => OutputMusicDancingMode, ref _outputMusicDancingMode, value); } }
        public int OutputBreathingSpeed { get => _outputBreathingSpeed; set { Set(() => OutputBreathingSpeed, ref _outputBreathingSpeed, value); } }
        public IColorPalette OutputCurrentActivePalette { get => _outputCurrentActivePalette; set { Set(() => OutputCurrentActivePalette, ref _outputCurrentActivePalette, value); } }
        public ILEDSetup OutputLEDSetup { get => _outputLEDSetup; set { Set(() => OutputLEDSetup, ref _outputLEDSetup, value); } }
        public bool IsInSpotEditWizard { get => _isInSpotEditWizard; set { Set(() => IsInSpotEditWizard, ref _isInSpotEditWizard, value); } }
        public string Geometry { get => _geometry; set { Set(() => Geometry, ref _geometry, value); } }
        public int OutputSmoothness { get => _outputSmoothness; set { Set(() => OutputSmoothness, ref _outputSmoothness, value); } }
        public int OutputMusicVisualizerFreq { get => _outputMusicVisualizerFreq; set { Set(() => OutputMusicVisualizerFreq, ref _outputMusicVisualizerFreq, value); } }

        public int OutputPaletteChasingPosition { get => _outputPaletteChasingPosition; set { Set(() => OutputPaletteChasingPosition, ref _outputPaletteChasingPosition, value); } }
        public int OutputScreenCaptureWBRed { get => _outputScreenCaptureWBRed; set { Set(() => OutputScreenCaptureWBRed, ref _outputScreenCaptureWBRed, value); } }
        public int OutputScreenCaptureWBGreen { get => _outputScreenCaptureWBGreen; set { Set(() => OutputScreenCaptureWBGreen, ref _outputScreenCaptureWBGreen, value); } }
        public int OutputScreenCaptureWBBlue { get => _outputScreenCaptureWBBlue; set { Set(() => OutputScreenCaptureWBBlue, ref _outputScreenCaptureWBBlue, value); } }
        public int OutputMusicSensitivity { get => _outputMusicSensitivity; set { Set(() => OutputMusicSensitivity, ref _outputMusicSensitivity, value); } }
        public int OutputScreenCapturePositionIndex { get => _outputScreenCapturePositionIndex; set { Set(() => OutputScreenCapturePositionIndex, ref _outputScreenCapturePositionIndex, value); } }

        public System.Drawing.Rectangle OutputRectangle { get => _outputRectangle; set { Set(() => OutputRectangle, ref _outputRectangle, value); } }
        public bool OutputIsLoadingProfile { get => _outputIsLoadingProfile; set { Set(() => OutputIsLoadingProfile, ref _outputIsLoadingProfile, value); } }
        public bool OutputIsBuildingLEDSetup { get => _outputIsBuildingLEDSetup; set { Set(() => OutputIsBuildingLEDSetup, ref _outputIsBuildingLEDSetup, value); } }
    }
}
