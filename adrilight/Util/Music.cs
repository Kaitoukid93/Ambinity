﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using adrilight.Util;
using System.Threading;
using NLog;
using Un4seen.BassWasapi;
using Un4seen.Bass;
using System.Windows;
using System.Diagnostics;
using adrilight.Spots;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;

namespace adrilight
{
    internal class Music :  IMusic
    {


        public static float[] _fft;
        public static int _lastlevel;
        public static int _hanctr;
        public static int volumeLeft;
        public static int volumeRight;
        public static int height = 0;
        public static int heightL = 0;
        public static int heightR = 0;
        private float[] lastSpectrumData;
        private Color[] fftColors;
        public WASAPIPROC _process;
        public static byte lastvolume = 0;
        public static byte volume = 0;
        public static int lastheight = 0;
        private float speed1 = 1.0F, speed2 = 0.20F, lightTime = 5.0F;
        private float minHeight = 50.0F;
        public static bool bump = false;

        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public Music(IOutputSettings outputSettings, IRainbowTicker rainbowTicker, IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {
            OutputSettings = outputSettings ?? throw new ArgumentNullException(nameof(outputSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
           // OutputSpotSet = outputSpotSet ?? throw new ArgumentException(nameof(outputSpotSet));


            RainbowTicker = rainbowTicker ?? throw new ArgumentNullException(nameof(rainbowTicker));
            MainViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));


            OutputSettings.PropertyChanged += PropertyChanged;
            GeneralSettings.PropertyChanged += PropertyChanged;
            MainViewModel.PropertyChanged += PropertyChanged;
            inSync = OutputSettings.OutputIsSystemSync;

            BassNet.Registration("saorihara93@gmail.com", "2X2831021152222");
            _process = new WASAPIPROC(Process);
            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            RefreshAudioState();
            _log.Info($"MusicColor Created");

        }
        //Dependency Injection//
        private IOutputSettings OutputSettings { get; }

        private MainViewViewModel MainViewModel { get; }
        private IRainbowTicker RainbowTicker { get; }
        private IGeneralSettings GeneralSettings { get; }
        private bool inSync { get; set; }
        //private IDeviceSpotSet OutputSpotSet { get; }

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
                case nameof(OutputSettings.OutputSelectedAudioDevice):

                    RefreshAudioDevice();
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

                Init();
                int deviceID = AudioDeviceID;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                bool result = BassWasapi.BASS_WASAPI_Init(deviceID, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index

                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    MessageBox.Show(error.ToString());
                }
                else
                {
                    //_initialized = true;
                    //  Bassbox.IsEnabled = false;
                }
                BassWasapi.BASS_WASAPI_Start();
                //BassWasapi.BASS_WASAPI_Init(-3, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "MusicColorCreator"
                };
                thread.Start();
            }
        }
        private void RefreshAudioDevice()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = OutputSettings.OutputIsEnabled && OutputSettings.OutputSelectedMode == 2;
            //var shouldBeRunning = DeviceSettings.TransferActive && DeviceSettings.SelectedEffect == 3;
            if (isRunning && shouldBeRunning)
            {

                _log.Debug("Refreshing the Music Color");
                Init();
                int deviceID = AudioDeviceID;
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
                bool result = BassWasapi.BASS_WASAPI_Init(deviceID, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero); // this is the function to init the device according to device index

                if (!result)
                {
                    var error = Bass.BASS_ErrorGetCode();
                    MessageBox.Show(error.ToString());
                }
                else
                {
                    //_initialized = true;
                    //  Bassbox.IsEnabled = false;
                }
                BassWasapi.BASS_WASAPI_Start();
                _log.Debug("Music Color Refreshed Successfully");
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

        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
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
                lastSpectrumData = new float[MainViewModel.IDMaxValue];
                fftColors = new Color[MainViewModel.IDMaxValue];
                //int counter = 0;
                List<byte> spectrumdata = new List<byte>();
                while (!token.IsCancellationRequested)
                {


                    //audio capture section//



                    bool isPreviewRunning = MainViewModel.IsVisualizerWindowOpen;
                    bool isLightingControlPreviewRunning = MainViewModel.IsSplitLightingWindowOpen;
                     GetCurrentFFTColorFrame(MainViewModel.IDMaxValue);
                    //if (spectrumdata == null) return;
                    if (isPreviewRunning)
                    MainViewModel.SetPreviewVisualizerFFT(lastSpectrumData, fftColors);
                    var brightnessMap = SpectrumCreator(lastSpectrumData, 0, 1, 1, 0, numLED);// get brightness map based on spectrum data

                    lock (OutputSettings.OutputLEDSetup.Lock)
                    {
                        
                        
                            
                        


                        int position = 0;
                        foreach (var spot in OutputSettings.OutputLEDSetup.Spots)
                        {



                            position = (int)RainbowTicker.StartIndex + spot.VID;
                            int n = 0;
                            if (position >= colorBank.Length)
                                n = position / colorBank.Length;
                            position = position - n * colorBank.Length; // run with VID


                            //var brightness = 0.5;/*brightnessMap[spot.VID];*/
                            var newColor = new OpenRGB.NET.Models.Color(colorBank[position].R, colorBank[position].G, colorBank[position].B);
                            var freq = spot.MID;
                            if (freq >= brightnessMap.Length)
                                freq = brightnessMap.Length - 1;
                            var outputColor = Brightness.applyBrightness(newColor, brightnessMap[freq], numLED, outputPowerMiliamps, outputPowerVoltage);
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

        public int _audioDeviceID = -1;
        public int AudioDeviceID {
            get
            {
                if (OutputSettings.OutputSelectedAudioDevice > AvailableAudioDevice.Count)
                {
                    System.Windows.MessageBox.Show("Last Selected Audio Device is not Available");
                    return -1;
                }
                else
                {
                    var currentDevice = AvailableAudioDevice.ElementAt(OutputSettings.OutputSelectedAudioDevice);

                    var array = currentDevice.Split(' ');
                    _audioDeviceID = Convert.ToInt32(array[0]);
                    return _audioDeviceID;
                }

            }
        }

        public void GetCurrentFFTColorFrame( int numFreq)
        {
            var minheight = (float)OutputSettings.OutputMusicSensitivity;
            List<byte> spectrumdata = new List<byte>();
            double senspercent = 0 / 100d;
            //audio capture section//
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048);// get channel fft data
            if (ret < -1) return;
            int x, y;
            int b0 = 0;
            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x = 0; x < numFreq; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (numFreq - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 250 - 4);
                if (y > 255) y = 255;
                if (y < 10) y = 0;
                spectrumdata.Add((byte)y);
                //spectrumdata[x] = (byte)((spectrumdata[x] * 6 + y * 2 + 7) / 8);

                //spectrumdata[x] = (byte)((byte)(spectrumdata[x] * senspercent) + spectrumdata[x]);
                //Smoothing out the value (take 5/8 of old value and 3/8 of new value to make finnal value)
             

            }
           
            int level = BassWasapi.BASS_WASAPI_GetLevel(); // Get level (VU metter) for Old AMBINO Device (remove in the future)
            if (level == _lastlevel && level != 0) _hanctr++;
            volumeLeft = (volumeLeft * 6 + Utils.LowWord32(level) * 2) / 8;
            volumeRight = (volumeRight * 6 + Utils.HighWord32(level) * 2) / 8;
            _lastlevel = level;

            
           
            for (int i = 0; i < numFreq; i++)
            {
                if (spectrumdata[i] > lastSpectrumData[i])
                {
                    lastSpectrumData[i] += speed1 * (spectrumdata[i] - lastSpectrumData[i]);

                    if (lastSpectrumData[i] >= minheight)
                    {
                        Color newClr = Blend(Color.FromRgb(255,0,0), Color.FromRgb(0, 255, 0), spectrumdata[i] - minheight);
                        fftColors[i] = Blend(Colors.Black, newClr, 255);
                        
                    }
                }

                if (spectrumdata[i] < lastSpectrumData[i])
                {
                    lastSpectrumData[i] -= speed2 * (lastSpectrumData[i] - spectrumdata[i]);

                    if (spectrumdata[i] + lightTime < lastSpectrumData[i] || lastSpectrumData[i] <= 0.1F)
                    {
                        Color newClr = Color.FromRgb(0,0,0);
                        fftColors[i] = Blend(Colors.Black, newClr, 255);
                        
                    }
                }

               

                
            }
            
        }

        public IList<string> _AvailableAudioDevice = new List<string>();
        public IList<string> AvailableAudioDevice {
            get
            {
                _AvailableAudioDevice.Clear();
                int devicecount = BassWasapi.BASS_WASAPI_GetDeviceCount();
                string[] devicelist = new string[devicecount];
                for (int i = 0; i < devicecount; i++)
                {

                    var devices = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);

                    if (devices.IsEnabled && devices.IsLoopback)
                    {
                        var device = string.Format("{0} - {1}", i, devices.name);

                        _AvailableAudioDevice.Add(device);
                    }

                }

                return _AvailableAudioDevice;
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
            else if (musicMode == 1)//vu mode
            {


                double percent = ((levelLeft + levelRight) / 2) / 16384;
                height = (int)(percent * numLED);
                int finalheight = (height * 4 + lastheight * 4 + 7) / 8;
                foreach (var brightness in brightnessMap.Take(finalheight))
                {

                    brightnessMap[counter++] = maxbrightness / 255.0;

                }


                for (int i = finalheight; i < numLED; i++)
                {

                    brightnessMap[counter++] = 0;

                }
                lastheight = finalheight;
            }
            else if (musicMode == 2)// End to End
            {
                double percent = ((levelLeft + levelRight) / 2) / 16384;
                height = (int)(percent * numLED);
                height = (int)(percent * numLED);
                int finalheight = (height * 4 + lastheight * 4 + 7) / 8;
                for (int i = 0; i < numLED / 2; i++)
                {
                    if (i <= finalheight / 2)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0.0;

                }


                for (int i = numLED / 2; i < numLED; i++)
                {
                    if (numLED - i <= finalheight / 2)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0.0;

                }
                lastheight = finalheight;
            }
            else if (musicMode == 3)//Push Pull
            {
                double percent = ((levelLeft + levelRight) / 2) / 16384;
                height = (int)(percent * numLED);
                height = (int)(percent * numLED);
                int finalheight = (height * 4 + lastheight * 4 + 7) / 8;
                for (int i = 0; i < numLED / 2; i++)
                {
                    if (i <= finalheight / 2)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0.0;

                }


                for (int i = numLED / 2; i < numLED; i++)
                {
                    if (numLED - i >= finalheight / 2)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0.0;

                }
                lastheight = finalheight;
            }
            else if (musicMode == 4)//Symetric VU
            {

                double percentleft = levelLeft / 16384;
                heightL = (int)(percentleft * numLED);
                double percentright = levelRight / 16384;
                heightR = (int)(percentright * numLED);


                for (int i = 0; i < numLED / 2; i++)
                {
                    if (i <= heightR)
                        brightnessMap[i] = maxbrightness;
                    else
                        brightnessMap[i] = 0.0;

                }


                for (int i = numLED / 2; i < numLED; i++)
                {
                    if (Math.Abs(numLED / 2 - i) <= heightL)
                        brightnessMap[i] = maxbrightness;
                    else
                        brightnessMap[i] = 0;

                }
            }
            else if (musicMode == 5)//Floating VU
            {
                double percentleft = levelLeft / 16384;
                heightL = (int)(percentleft * numLED);
                double percentright = levelRight / 16384;
                heightR = (int)(percentright * numLED);

                for (int i = 0; i < numLED / 2; i++)
                {
                    if (Math.Abs(0 - i) <= heightR)
                        brightnessMap[i] = 0.0;
                    else
                        brightnessMap[i] = maxbrightness / 255.0;

                }


                for (int i = numLED / 2; i < numLED; i++)
                {
                    if (Math.Abs(numLED / 2 - i) <= heightL)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0;

                }
            }
            else if (musicMode == 6)//Center VU
            {
                double percentleft = levelLeft / 16384;
                heightL = (int)(percentleft * numLED);
                double percentright = levelRight / 16384;
                heightR = (int)(percentright * numLED);

                for (int i = numLED / 2; i > 0; i--)
                {
                    if (Math.Abs(numLED / 2 - i) <= heightL)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0.0;

                }


                for (int i = numLED / 2; i < numLED; i++)
                {
                    if (Math.Abs(numLED / 2 - i) <= heightR)
                        brightnessMap[i] = maxbrightness / 255.0;
                    else
                        brightnessMap[i] = 0;

                }
            }

            else if (musicMode == 7)// jumping bass?
            {
                Random random = new Random();
                var equalizer = EqualizerPick(0, numLED);
                for (int i = 0; i < brightnessMap.Count(); i++)
                {

                    brightnessMap[i] = fft[equalizer[i]] / 255.0;

                }

            }
            return brightnessMap;

        }

        Color Blend(Color colorA, Color colorB, float a)
        {
            if (a > 255.0F) { a = 255.0F; }
            float amount = 1.0F - (a / 255.0F);

            float r = ((colorA.R * amount) + colorB.R * (1 - amount));
            float g = ((colorA.G * amount) + colorB.G * (1 - amount));
            float b = ((colorA.B * amount) + colorB.B * (1 - amount));

            if (r < 0) { r = 0; }
            if (g < 0) { g = 0; }
            if (b < 0) { b = 0; }

            return Color.FromRgb((byte)Math.Round(r), (byte)Math.Round(g), (byte)Math.Round(b));
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

        public static int[] EqualizerPick(int mode, int numLED)
        {


            int[] equalizerPick = new int[numLED];
            if (mode == 0)
            {
                for (int i = 0; i < numLED; i++)
                {
                    if (i < numLED / 4)
                    {
                        equalizerPick[i] = 2;
                    }
                    if (i > numLED / 2 && i < 3 * numLED / 4)
                    {
                        equalizerPick[i] = 2;
                    }
                    if (i >= numLED / 4 && i < numLED / 2)
                    {
                        equalizerPick[i] = numLED / 2;
                    }
                    if (i >= 3 * numLED / 4)
                    {
                        equalizerPick[i] = numLED / 2;
                    }
                }

            }



            return equalizerPick;

        }

        private void Init()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }




        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }



    }
}
