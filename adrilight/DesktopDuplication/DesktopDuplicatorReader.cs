using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using adrilight.DesktopDuplication;
using NLog;
using Polly;
using System.Linq;
using System.Windows.Media.Imaging;
using adrilight.ViewModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using adrilight.Resources;
using adrilight.Util;
using adrilight.Spots;

namespace adrilight
{
    internal class DesktopDuplicatorReader : IDesktopDuplicatorReader
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopDuplicatorReader(IGeneralSettings userSettings,
            IDeviceSettings deviceSettings,
            IDeviceSpotSet deviceSpotSet ,
            MainViewViewModel mainViewViewModel,
            IDesktopFrame desktopFrame,
             ISecondDesktopFrame secondDesktopFrame,
             IThirdDesktopFrame thirdDesktopFrame
            )
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            DesktopFrame = desktopFrame ?? throw new ArgumentNullException(nameof(desktopFrame));
            SecondDesktopFrame = secondDesktopFrame ?? throw new ArgumentNullException(nameof(secondDesktopFrame));
            ThirdDesktopFrame = thirdDesktopFrame ?? throw new ArgumentNullException(nameof(thirdDesktopFrame));

            // GraphicAdapter = graphicAdapter;
            // Output = output;
            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            // SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);

            UserSettings.PropertyChanged += PropertyChanged;
            DeviceSettings.PropertyChanged += PropertyChanged;
            // SettingsViewModel.PropertyChanged += PropertyChanged;
            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                
                case nameof(UserSettings.ShouldbeRunning):
                case nameof(DeviceSettings.SelectedEffect):

                    RefreshCapturingState();
                    break;

                case nameof(UserSettings.SelectedDisplay):
                case nameof(UserSettings.SelectedAdapter):
                    RefreshCaptureSource();
                    break;
            }
        }

        public bool IsRunning { get; private set; } = false;
        public bool NeededRefreshing { get; private set; } = false;
        private MainViewViewModel MainViewViewModel { get; }
        private CancellationTokenSource _cancellationTokenSource;

        private Thread _workerThread;
       // public int GraphicAdapter;
       // public int Output;

       public void RefreshCaptureSource()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = UserSettings.ShouldbeRunning;
            //  var shouldBeRefreshing = NeededRefreshing;
            if (isRunning && shouldBeRunning)
            {
                //start it

                IsRunning = false;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();

            }
        }
        public void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = DeviceSettings.TransferActive && DeviceSettings.SelectedEffect == 0;
            //  var shouldBeRefreshing = NeededRefreshing;



            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the capturing");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
               

            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the capturing");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                _workerThread.Start();


            }

        }


  

        private IGeneralSettings UserSettings { get; }
        private IDeviceSettings DeviceSettings { get; }
        private IDeviceSpotSet DeviceSpotSet { get; }
        private IDesktopFrame DesktopFrame { get; }
        private ISecondDesktopFrame SecondDesktopFrame { get; }
        private IThirdDesktopFrame ThirdDesktopFrame { get; }




        private readonly Policy _retryPolicy;

        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {
                
                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming
                DeviceSpotSet.IndicateMissingValues();
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }




        public void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
            NeededRefreshing = false;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
           

            try
            {



                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetNextFrame(image));
                    TraceFrameDetails(newImage);
                    var width = DeviceSettings.DeviceRectWidth;
                    var height = DeviceSettings.DeviceRectHeight;
                    var x = DeviceSettings.DeviceRectLeft;
                    var y = DeviceSettings.DeviceRectTop;
                    var brightness = DeviceSettings.Brightness/100d;

                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;

                    //bool isPreviewRunning = SettingsViewModel.IsSettingsWindowOpen && SettingsViewModel.IsPreviewTabOpen;
                    //if (isPreviewRunning)
                    //{
                    //   MainViewViewModel.SetPreviewImage(image);
                  

                    image.LockBits(new Rectangle(x, y, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb, bitmapData);


                    lock (DeviceSpotSet.Lock)
                    {
                        var useLinearLighting = UserSettings.UseLinearLighting==0;

                        //var imageRectangle = new Rectangle(0, 0, image.Width, image.Height);

                        //if (imageRectangle.Width != DeviceSpotSet.ExpectedScreenWidth || imageRectangle.Height != DeviceSpotSet.ExpectedScreenHeight)
                        //{
                        //    //the screen was resized or this is some kind of powersaving state
                        //    DeviceSpotSet.IndicateMissingValues();

                        //    continue;
                        //}
                        //else
                        //{
                            Parallel.ForEach(DeviceSpotSet.Spots
                                , spot =>
                                {
                                    const int numberOfSteps = 15;
                                    int stepx = Math.Max(1, spot.Rectangle.Width / numberOfSteps);
                                    int stepy = Math.Max(1, spot.Rectangle.Height / numberOfSteps);

                                    GetAverageColorOfRectangularRegion(spot.Rectangle, stepy, stepx, bitmapData,
                                        out int sumR, out int sumG, out int sumB, out int count);

                                    var countInverse = 1f / count;

                                    ApplyColorCorrections(sumR * countInverse, sumG * countInverse, sumB * countInverse
                                        , out byte finalR, out byte finalG, out byte finalB, useLinearLighting
                                        , UserSettings.SaturationTreshold, spot.Red, spot.Green, spot.Blue);

                                    var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);

                                    var semifinalSpotColor = Brightness.applyBrightness(spotColor, brightness);
                                    ApplySmoothing(semifinalSpotColor.R, semifinalSpotColor.G, semifinalSpotColor.B
                                        , out byte RealfinalR, out byte RealfinalG, out byte RealfinalB,
                                     spot.Red, spot.Green, spot.Blue);
                                    spot.SetColor(RealfinalR, RealfinalG, RealfinalB, true);

                                });
                        //}

                    }



                    image.UnlockBits(bitmapData);

                    int minFrameTimeInMs = 1000 / UserSettings.LimitFps;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                }
            }


            finally
            {
                image?.Dispose();
                
                _log.Debug("Stopped Desktop Duplication Reader.");
                IsRunning = false;
                GC.Collect();
            }
        }













        private int? _lastObservedHeight;
        private int? _lastObservedWidth;

        private void TraceFrameDetails(Bitmap image)
        {
            //there are many frames per second and we need to extract useful information and only log those!
            if (image == null)
            {
                //if the frame is null, this can mean two things. the timeout from the desktop duplication api was reached
                //before the monitor content changed or there was some other error.
            }
            else
            {
                if (_lastObservedHeight != null && _lastObservedWidth != null
                    && (_lastObservedHeight != image.Height || _lastObservedWidth != image.Width))
                {
                    _log.Debug("The frame size changed from {0}x{1} to {2}x{3}"
                        , _lastObservedWidth, _lastObservedHeight
                        , image.Width, image.Height);

                }
                _lastObservedWidth = image.Width;
                _lastObservedHeight = image.Height;
            }
        }

        private void ApplyColorCorrections(float r, float g, float b, out byte finalR, out byte finalG, out byte finalB, bool useLinearLighting, byte saturationTreshold
         , byte lastColorR, byte lastColorG, byte lastColorB)
        {
            if (lastColorR == 0 && lastColorG == 0 && lastColorB == 0)
            {
                //if the color was black the last time, we increase the saturationThreshold to make flickering more unlikely
                saturationTreshold += 2;
            }
            if (r <= saturationTreshold && g <= saturationTreshold && b <= saturationTreshold)
            {
                //black
                finalR = finalG = finalB = 0;
                return;
            }

            //"white" on wall was 66,68,77 without white balance
            //white balance
            //todo: introduce settings for white balance adjustments
            r *= UserSettings.WhitebalanceRed / 100f;
            g *= UserSettings.WhitebalanceGreen / 100f;
            b *= UserSettings.WhitebalanceBlue / 100f;

            if (!useLinearLighting)
            {
                //apply non linear LED fading ( http://www.mikrocontroller.net/articles/LED-Fading )
                finalR = FadeNonLinear(r);
                finalG = FadeNonLinear(g);
                finalB = FadeNonLinear(b);
            }
            else
            {
                //output
                finalR = (byte)r;
                finalG = (byte)g;
                finalB = (byte)b;
            }
        }
        private void ApplySmoothing(float r, float g, float b, out byte semifinalR, out byte semifinalG, out byte semifinalB,
           byte lastColorR, byte lastColorG, byte lastColorB)
        {
            int smoothingFactor = UserSettings.SmoothFactor;
           

            semifinalR = (byte)((r + smoothingFactor * lastColorR) / (smoothingFactor + 1));
            semifinalG = (byte)((g + smoothingFactor * lastColorG) / (smoothingFactor + 1));
            semifinalB = (byte)((b + smoothingFactor * lastColorB) / (smoothingFactor + 1));
        }


        private readonly byte[] _nonLinearFadingCache = Enumerable.Range(0, 2560)
            .Select(n => FadeNonLinearUncached(n / 10f))
            .ToArray();

        private byte FadeNonLinear(float color)
        {
            var cacheIndex = (int)(color * 10);
            return _nonLinearFadingCache[Math.Min(2560 - 1, Math.Max(0, cacheIndex))];
        }

        private static byte FadeNonLinearUncached(float color)
        {
            const float factor = 80f;
            return (byte)(256f * ((float)Math.Pow(factor, color / 256f) - 1f) / (factor - 1));
        }

        private Bitmap GetNextFrame(Bitmap ReusableBitmap)
        {

           
         

            try
            {
                byte[] CurrentFrame = null;
                Bitmap DesktopImage;
                switch(DeviceSettings.SelectedDisplay)
                {
                    case 0:
                         CurrentFrame = DesktopFrame.Frame;
                        break;
                    case 1:
                         CurrentFrame = SecondDesktopFrame.Frame;
                        break;
                    case 2:
                         CurrentFrame = ThirdDesktopFrame.Frame;
                        break;
                }    
                
                if (CurrentFrame == null)
                {
                    return null;
                }
                else
                {
                    if (ReusableBitmap != null&&ReusableBitmap.Width==DesktopFrame.FrameWidth&&ReusableBitmap.Height==DesktopFrame.FrameHeight)
                {
                        DesktopImage = ReusableBitmap;
                        
                }
                else
                {
                        DesktopImage = new Bitmap(DesktopFrame.FrameWidth, DesktopFrame.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                 }
                
                var DesktopImageBitmapData = DesktopImage.LockBits(new Rectangle(0, 0, DesktopFrame.FrameWidth, DesktopFrame.FrameHeight), ImageLockMode.WriteOnly, DesktopImage.PixelFormat);
                IntPtr pixelAddress = DesktopImageBitmapData.Scan0;
                
                    
                    
                    
                        Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);

                    

                DesktopImage.UnlockBits(DesktopImageBitmapData);

                return DesktopImage;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "_outputDuplication is null" && ex.Message != "Access Lost, resolution might be changed" && ex.Message != "Invalid call, might be retrying" && ex.Message != "Failed to release frame.")
                {
                    _log.Error(ex, "GetNextFrame() failed.");

                    // throw;
                }
                else if (ex.Message == "Access Lost, resolution might be changed")
                {
                    _log.Error(ex, "Access Lost, retrying");

                }
                else if (ex.Message == "Invalid call, might be retrying")
                {
                    _log.Error(ex, "Invalid Call Lost, retrying");
                }
                else if (ex.Message == "Failed to release frame.")
                {
                    _log.Error(ex, "Failed to release frame.");
                }
                else
                {
                    throw new DesktopDuplicationException("Unknown Device Error", ex);
                }


                //_desktopDuplicator.Dispose();
                //_desktopDuplicator = null;
                GC.Collect();
                return null;
            }
        }

        public void Stop()
        {
            _log.Debug("Stop called.");
            if (_workerThread == null) return;
           
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }
   

        private unsafe void GetAverageColorOfRectangularRegion(Rectangle spotRectangle, int stepy, int stepx, BitmapData bitmapData, out int sumR, out int sumG,
            out int sumB, out int count)
        {
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            var stepCount = spotRectangle.Width / stepx;
            var stepxTimes4 = stepx * 4;
            for (var y = spotRectangle.Top; y < spotRectangle.Bottom; y += stepy)
            {
                byte* pointer = (byte*)bitmapData.Scan0 + bitmapData.Stride * y + 4 * spotRectangle.Left;
                for (int i = 0; i < stepCount; i++)
                {
                    sumB += pointer[0];
                    sumG += pointer[1];
                    sumR += pointer[2];

                    pointer += stepxTimes4;
                }
                count += stepCount;
            }
        }

    }
}