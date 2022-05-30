using OpenRGB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using adrilight.ViewModel;
using System.Threading;
using NLog;
using System.Threading.Tasks;
using System.Diagnostics;
using adrilight.Spots;

namespace adrilight
{
    internal class Rainbow : IRainbow
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Rainbow(IOutputSettings outputSettings, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
           // OutputSpotSet = outputSpotSet ?? throw new ArgumentException(nameof(outputSpotSet));


            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
           MainViewViewModel= mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            

            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;
            RefreshColorState();
            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }
       
        private MainViewViewModel MainViewViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }
        // private IDeviceSpotSet OutputSpotSet { get; }

        private Color[] colorBank = new Color[256];
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
               
                
                    RefreshColorState();
                    break;
                case nameof(OutputSettings.OutputCurrentActivePalette):
                case nameof(OutputSettings.IsInSpotEditWizard):
                case nameof(OutputSettings.OutputIsSystemSync):
                

                    ColorPaletteChanged();
                    break;


            }
        }
       

        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 1 && OutputSettings.IsInSpotEditWizard == false;

            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Rainbow Color");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Rainbow Color");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "RainbowColorCreator" + OutputSettings.OutputUniqueID
                };
                thread.Start();
            }
        }
        private void ColorPaletteChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 1 && OutputSettings.IsInSpotEditWizard == false;
            var isInEditWizard = OutputSettings.IsInSpotEditWizard;

            if (isRunning && shouldBeRunning)
            {
                // rainbow is running and we need to change the color bank
                colorBank = GetColorGradientfromPalette(OutputSettings.OutputCurrentActivePalette.Colors, GeneralSettings.SystemRainbowMaxTick).ToArray();
                inSync = OutputSettings.OutputIsSystemSync;
                //if(isInEditWizard)
                //    colorBank = GetColorGradientfromPalette(DefaultColorCollection.black).ToArray();
            }

        }





        public void Run(CancellationToken token)

        {

            if (IsRunning) throw new Exception(" Rainbow Color is already running!");

            IsRunning = true;

            _log.Debug("Started Rainbow Color.");

            try
            {



                var numLED = OutputSettings.OutputLEDSetup.Spots.Length*OutputSettings.LEDPerSpot*OutputSettings.LEDPerLED;
                var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;
                var effectSpeed = OutputSettings.OutputPaletteSpeed;
                var frequency = OutputSettings.OutputPaletteBlendStep;
                var colorNum = GeneralSettings.SystemRainbowMaxTick;
                Color[] paletteSource = OutputSettings.OutputCurrentActivePalette.Colors;
                colorBank = GetColorGradientfromPalette(paletteSource,colorNum).ToArray();
                double StartIndex=0d;
                int OutputStartIndex = 0;
                
              
                while (!token.IsCancellationRequested)
                {
                    bool isPreviewRunning = MainViewViewModel.IsSplitLightingWindowOpen;
                    double speed = OutputSettings.OutputPaletteSpeed / 5d;
                    StartIndex += speed;
                    if (StartIndex > GeneralSettings.SystemRainbowMaxTick)
                    {
                        StartIndex = 0;
                    }
                    if(inSync)
                    {
                        OutputStartIndex = (int)RainbowTicker.StartIndex;
                    }
                    else
                    {
                        OutputStartIndex = (int)StartIndex;
                    }
                    
                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {

                        int position = 0;
                        foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                        {

                                //caculate the overlap 
                                
                                position = OutputStartIndex + spot.VID;
                                int n = 0;
                                if(position>=colorBank.Length)
                                n = position / colorBank.Length;
                                position -= n*colorBank.Length; // run with VID
                            
                         
                            var brightness = OutputSettings.OutputBrightness / 100d;
                            var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                            var outputColor=Brightness.applyBrightness(newColor, brightness, numLED, outputPowerMiliamps, outputPowerVoltage);    
                            if(!OutputSettings.IsInSpotEditWizard)
                            spot.SetColor(outputColor.R, outputColor.G, outputColor.B, isPreviewRunning);

                        }


                    }
                    Thread.Sleep(10);


                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");


            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {

                _log.Debug("Stopped Rainbow Color Creator.");
                IsRunning = false;

            }


        }






        [Obsolete]
        private static Color GetColorByOffset(GradientStopCollection collection, double position)
        {
            double offset = position / 1000.0;

            GradientStop[] stops = collection.OrderBy(x => x.Offset).ToArray();
            if (offset <= 0) return stops[0].Color;
            if (offset >= 1) return stops[stops.Length - 1].Color;
            GradientStop left = stops[0], right = null;
            foreach (GradientStop stop in stops)
            {
                if (stop.Offset >= offset)
                {
                    right = stop;
                    break;
                }
                left = stop;
            }
            Debug.Assert(right != null);
            offset = Math.Round((offset - left.Offset) / (right.Offset - left.Offset), 2);

            byte r = (byte)((right.Color.R - left.Color.R) * offset + left.Color.R);
            byte g = (byte)((right.Color.G - left.Color.G) * offset + left.Color.G);
            byte b = (byte)((right.Color.B - left.Color.B) * offset + left.Color.B);
            return Color.FromRgb(r, g, b);
        }
        [Obsolete]
        public GradientStopCollection GradientPaletteColor(Color[] ColorCollection)
        {

            GradientStopCollection gradientPalette = new GradientStopCollection(16);
            gradientPalette.Add(new GradientStop(ColorCollection[0], 0.00));
            gradientPalette.Add(new GradientStop(ColorCollection[1], 0.066));
            gradientPalette.Add(new GradientStop(ColorCollection[2], 0.133));
            gradientPalette.Add(new GradientStop(ColorCollection[3], 0.199));
            gradientPalette.Add(new GradientStop(ColorCollection[4], 0.265));
            gradientPalette.Add(new GradientStop(ColorCollection[5], 0.331));
            gradientPalette.Add(new GradientStop(ColorCollection[6], 0.397));
            gradientPalette.Add(new GradientStop(ColorCollection[7], 0.464));
            gradientPalette.Add(new GradientStop(ColorCollection[8], 0.529));
            gradientPalette.Add(new GradientStop(ColorCollection[9], 0.595));
            gradientPalette.Add(new GradientStop(ColorCollection[10], 0.661));
            gradientPalette.Add(new GradientStop(ColorCollection[11], 0.727));
            gradientPalette.Add(new GradientStop(ColorCollection[12], 0.793));
            gradientPalette.Add(new GradientStop(ColorCollection[13], 0.859));
            gradientPalette.Add(new GradientStop(ColorCollection[14], 0.925));
            gradientPalette.Add(new GradientStop(ColorCollection[15], 1));

            return gradientPalette;
        }


        public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
        {
            if (totalNumberOfColors < 2)
            {
                throw new ArgumentException("Gradient cannot have less than two colors.", nameof(totalNumberOfColors));
            }
            var colorList = new List<Color>();
            double diffA = to.A - from.A;
            double diffR = to.R - from.R;
            double diffG = to.G - from.G;
            double diffB = to.B - from.B;

            var steps = totalNumberOfColors - 1;

            var stepA = diffA / steps;
            var stepR = diffR / steps;
            var stepG = diffG / steps;
            var stepB = diffB / steps;



            for (var i = 1; i < steps; ++i)
            {
                colorList.Add(Color.FromArgb(
                     (byte)(c(from.A, stepA)),
                     (byte)(c(from.R, stepR)),
                     (byte)(c(from.G, stepG)),
                     (byte)(c(from.B, stepB))));

                int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }
            return colorList;

        }
        public static IEnumerable<Color> GetColorGradientfromPalette(Color[] colorCollection, int colorNum)
        {
            var colors = new List<Color>();
            int colorPerGap = colorNum / (colorCollection.Count()-1);
            
            for (int i = 0; i < colorCollection.Length-1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            int remainTick = colorNum - colors.Count();
            colors = colors.Concat(colors.Take(remainTick).ToList()).ToList();
            return colors;
        }

        // predefined color palette
        public static Color[] rainbow = {
             Color.FromRgb (255,0,25),
             Color.FromRgb (255,172,0),
             Color.FromRgb (255,172,0),
             Color.FromRgb (186,255,0),
             Color.FromRgb (186,255,0),
             Color.FromRgb (0,255,51),
             Color.FromRgb (0,255,51),
             Color.FromRgb (0,255,245),
             Color.FromRgb (0,255,245),
             Color.FromRgb (0,102,255),
             Color.FromRgb (0,102,255),
             Color.FromRgb (92,0,255),
             Color.FromRgb (92,0,255),
             Color.FromRgb (232,0,255),
             Color.FromRgb (232,0,255),
             Color.FromRgb (255,0,25)
        };
        public static Color[] party = {
             Color.FromRgb (88,53,148),
             Color.FromRgb (129,39,122),
             Color.FromRgb (181,30,78),
             Color.FromRgb (228,30,38),
             Color.FromRgb (0,255,0),
             Color.FromRgb (183,74,38),
             Color.FromRgb (170,121,43),
             Color.FromRgb (169,171,54),
             Color.FromRgb (170,87,38),
             Color.FromRgb (220,39,38),
             Color.FromRgb (237,28,36),
             Color.FromRgb (194,31,65),
             Color.FromRgb (140,36,114),
             Color.FromRgb (96,45,144),
             Color.FromRgb (67,70,157),
             Color.FromRgb (88,53,148)

        };
        public static Color[] cloud = {
             Color.FromRgb (0,152,255),
             Color.FromRgb (0,104,255),
             Color.FromRgb (0,62,216),
             Color.FromRgb (4,54,178),
             Color.FromRgb (0,43,150),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (220,245,255),
             Color.FromRgb (202,243,255),
             Color.FromRgb (123,224,255),
             Color.FromRgb (16,170,255),
             Color.FromRgb (0,152,255)

    };

        public static Color[] forest = {
             Color.FromRgb (134,255,64),
             Color.FromRgb (53,214,0),
             Color.FromRgb (0,165,39),
             Color.FromRgb (3,114,50),
             Color.FromRgb (1,96,40),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (232,255,220),
             Color.FromRgb (209,255,184),
             Color.FromRgb (181,255,133),
             Color.FromRgb (170,255,80),
             Color.FromRgb (134,255,64)

    };

        public static Color[] sunset = {
             Color.FromRgb (244,126,32),
             Color.FromRgb (239,76,35),
             Color.FromRgb (237,52,36),
             Color.FromRgb (236,30,36),
             Color.FromRgb (169,30,34),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (246,235,15),
             Color.FromRgb (255,206,3),
             Color.FromRgb (252,181,20),
             Color.FromRgb (247,150,29),
             Color.FromRgb (244,126,32)

    };
        public static Color[] scarlet = {
             Color.FromRgb (236,34,143),
             Color.FromRgb (236,24,94),
             Color.FromRgb (236,30,36),
             Color.FromRgb (203,32,39),
             Color.FromRgb (146,26,29),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (193,111,172),
             Color.FromRgb (194,86,159),
             Color.FromRgb (214,93,161),
             Color.FromRgb (202,75,154),
             Color.FromRgb (236,34,143)

    };
        public static Color[] aurora = {
             Color.FromRgb (0,255,133),
             Color.FromRgb (0,249,255),
             Color.FromRgb (0,146,255),
             Color.FromRgb (0,104,255),
             Color.FromRgb (0,73,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (187,255,0),
             Color.FromRgb (137,255,0),
             Color.FromRgb (100,255,0),
             Color.FromRgb (94,255,0),
             Color.FromRgb (0,255,133)

    };
        public static Color[] france = {
             Color.FromRgb (255,0,19),
             Color.FromRgb (255,0,19),
             Color.FromRgb (255,0,19),
             Color.FromRgb (0,92,255),
             Color.FromRgb (0,92,255),
             Color.FromRgb (0,92,255),
             Color.FromRgb (0,92,255),
             Color.FromRgb (0,92,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,255,255),
             Color.FromRgb (255,0,19),
             Color.FromRgb (255,0,19),
             Color.FromRgb (255,0,19)

    };
        public static Color[] lemon = {
             Color.FromRgb (206,255,0),
             Color.FromRgb (144,255,0),
             Color.FromRgb (112,255,0),
             Color.FromRgb (19,255,0),
             Color.FromRgb (0,255,78),
             Color.FromRgb (246,235,15),
             Color.FromRgb (206,255,0),
             Color.FromRgb (144,255,0),
             Color.FromRgb (19,255,0),
             Color.FromRgb (0,255,78),
             Color.FromRgb (246,235,15),
             Color.FromRgb (246,235,15),
             Color.FromRgb (255,206,3),
             Color.FromRgb (255,255,0),
             Color.FromRgb (231,255,0),
             Color.FromRgb (206,255,0)

    };

        public static Color[] badtrip = {
             Color.FromRgb (34,34,242),
             Color.FromRgb (36,142,237),
             Color.FromRgb (0,189,255),
             Color.FromRgb (0,231,255),
             Color.FromRgb (0,255,242),
             Color.FromRgb (174,0,255),
             Color.FromRgb (99,0,255),
             Color.FromRgb (34,34,242),
             Color.FromRgb (34,34,242),
             Color.FromRgb (36,142,237),
             Color.FromRgb (0,189,255),
             Color.FromRgb (0,189,255),
             Color.FromRgb (0,255,242),
             Color.FromRgb (174,0,255),
             Color.FromRgb (99,0,255),
             Color.FromRgb (34,34,242)

    };
        public static Color[] police = {
             Color.FromRgb (18,0,255),
             Color.FromRgb (18,0,255),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (255,0,37),
             Color.FromRgb (255,0,37),
             Color.FromRgb (255,0,37),
             Color.FromRgb (255,0,37),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (0,0,0),
             Color.FromRgb (18,0,255),
             Color.FromRgb (18,0,255)

    };
        public static Color[] iceandfire = {
             Color.FromRgb (0,255,224),
             Color.FromRgb (0,255,224),
             Color.FromRgb (141,255,249),
             Color.FromRgb (141,255,249),
             Color.FromRgb (255,176,0),
             Color.FromRgb (255,176,0),
             Color.FromRgb (255,109,0),
             Color.FromRgb (255,109,0),
             Color.FromRgb (255,109,0),
             Color.FromRgb (255,109,0),
             Color.FromRgb (255,176,0),
             Color.FromRgb (255,176,0),
             Color.FromRgb (141,255,249),
             Color.FromRgb (141,255,249),
             Color.FromRgb (0,255,224),
             Color.FromRgb (0,255,224)

    };

        //Custom color by color picker value
        public Color[] custom = new Color[16];
        //public void GetCustomColor()
        //{
        //    custom = DeviceSettings.CustomZone;


        //}


    }
}
