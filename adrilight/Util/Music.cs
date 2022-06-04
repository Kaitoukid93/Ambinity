using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using System.Threading;
using NLog;
using System.Windows;
using System.Diagnostics;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;

namespace adrilight
{
    internal class Music : IMusic
    {



        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Music(IOutputSettings outputSettings, IAudioFrame audioFrames, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            AudioFrames = audioFrames ?? throw new ArgumentException(nameof(audioFrames));
            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;

            RefreshAudioState();
            _log.Info($"MusicColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }
        private IAudioFrame AudioFrames { get; set; }
        private MainViewViewModel MainViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }


        private Color[] colorBank = new Color[256];

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OutputSettings.OutputIsEnabled):
                case nameof(OutputSettings.OutputSelectedMode):
                case nameof(MainViewModel.IsVisualizerWindowOpen):
                    RefreshAudioState();
                    break;
                case nameof(OutputSettings.OutputCurrentActivePalette):
                case nameof(OutputSettings.IsInSpotEditWizard):
                    ColorPaletteChanged();
                    break;



            }

        }
        private void RefreshAudioState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 2;


            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Music Color");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                // Free();
            }

            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Music Color");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "MusicColorCreator"
                };
                thread.Start();
            }
        }

        private void ColorPaletteChanged()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 2;

            if (isRunning && shouldBeRunning)
            {
                // rainbow is running and we need to change the color bank
                colorBank = GetColorGradientfromPalette(OutputSettings.OutputCurrentActivePalette.Colors, GeneralSettings.SystemRainbowMaxTick).ToArray();
                inSync = OutputSettings.OutputIsSystemSync;
            }

        }




        public void Run(CancellationToken token)

        {


            if (IsRunning) throw new Exception(" Music Color is already running!");

            IsRunning = true;

            _log.Debug("Started Music Color.");


            try
            {


                var colorNum = GeneralSettings.SystemRainbowMaxTick;
                Color[] paletteSource = OutputSettings.OutputCurrentActivePalette.Colors;
                colorBank = GetColorGradientfromPalette(paletteSource, colorNum).ToArray();
                int musicMode = OutputSettings.OutputSelectedMusicMode;
                var outputPowerVoltage = OutputSettings.OutputPowerVoltage;
                var outputPowerMiliamps = OutputSettings.OutputPowerMiliamps;
                var numLED = OutputSettings.OutputLEDSetup.Spots.Length * OutputSettings.LEDPerSpot * OutputSettings.LEDPerLED;


                while (!token.IsCancellationRequested)
                {

                    bool isLightingControlPreviewRunning = MainViewModel.IsSplitLightingWindowOpen;
                    var fft = new float[32];
                    if (AudioFrames.FFT != null)
                        fft = AudioFrames.FFT;
                    var brightnessMap = SpectrumCreator(fft, 0, 1, 1, 0, 32);// get brightness map based on spectrum data
                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {
                        int position = 0;
                        foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                        {
                            position = (int)RainbowTicker.StartIndex + spot.MID;
                            int n = 0;
                            if (position >= colorBank.Length)
                                n = position / colorBank.Length;
                            position = position - n * colorBank.Length; // run with VID


                            //var brightness = 0.5;/*brightnessMap[spot.VID];*/
                            var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                            var freq = spot.MID;
                            var actualFreq = 32 * ((double)freq / 1023d);
                            var brightnessCap = OutputSettings.OutputBrightness / 100d;
                            var actualBrightness = brightnessMap[(int)actualFreq] * brightnessCap;
                            var outputColor = Brightness.applyBrightness(newColor, actualBrightness, numLED, outputPowerMiliamps, outputPowerVoltage);
                            ApplySmoothing(outputColor.R, outputColor.G, outputColor.B, out byte FinalR, out byte FinalG, out byte FinalB, spot.Red, spot.Green, spot.Blue);
                            spot.SetColor(FinalR, FinalG, FinalB, isLightingControlPreviewRunning);

                        }
                    }
                    Thread.Sleep(5);


                }
            }

            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                // return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");



                //allow the system some time to recover
                Thread.Sleep(500);
            }
            finally
            {


                _log.Debug("Stopped MusicColor Color Creator.");
                IsRunning = false;
            }



        }


        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
        byte lastColorR, byte lastColorG, byte lastColorB)
        {
            ;

            semifinalR = (byte)((r + 3 * lastColorR) / (3 + 1));
            semifinalG = (byte)((g + 3 * lastColorG) / (3 + 1));
            semifinalB = (byte)((b + 3 * lastColorB) / (3 + 1));
        }


        public static double[] SpectrumCreator(float[] fft, int sensitivity, double levelLeft, double levelRight, int musicMode, int numLED)//create brightnessmap based on input fft or volume
        {

            int counter = 0;
            int factor = numLED / fft.Length;
            byte maxbrightness = 255;
            double[] brightnessMap = new double[numLED];

            //this function take the input as frequency and output the color but the brightness change as the frequency band's value
            if (musicMode == 0)//equalizer mode, each block of LED is respond to 1 band of frequency spectrum
            {

                for (int i = 0; i < fft.Length; i++)
                {

                    brightnessMap[counter++] = (double)fft[i] / 255.0;

                }

            }

            return brightnessMap;

        }


        public static IEnumerable<Color> GetColorGradientfromPalette(Color[] colorCollection, int colorNum)
        {
            var colors = new List<Color>();
            int colorPerGap = colorNum / (colorCollection.Count() - 1);

            for (int i = 0; i < colorCollection.Length - 1; i++)
            {
                var gradient = GetColorGradient(colorCollection[i], colorCollection[i + 1], colorPerGap);
                colors = colors.Concat(gradient).ToList();
            }
            int remainTick = colorNum - colors.Count();
            colors = colors.Concat(colors.Take(remainTick).ToList()).ToList();
            return colors;
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

    }
}
