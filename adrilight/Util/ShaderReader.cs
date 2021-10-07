using adrilight.Extensions;
using adrilight.Spots;
using NLog;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace adrilight.Util
{
    internal class ShaderReader :IShaderReader
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        public ShaderReader(IGeneralSettings generalSettings, IDeviceSpotSet deviceSpotSet, IDeviceSettings deviceSettings, IShaderEffect shaderEffect)
        {

            ShaderEffect = shaderEffect ?? throw new ArgumentNullException(nameof(shaderEffect));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);
            GeneralSettings.PropertyChanged += PropertyChanged;
            DeviceSettings.PropertyChanged += DevicePropertyChanged;



            RefreshReadingState();

            _log.Info($"ShaderReader created.");
        }
        public bool IsRunning { get; set; }
        private IGeneralSettings GeneralSettings { get; }
        private IDeviceSettings DeviceSettings { get; }
        private IDeviceSpotSet DeviceSpotSet { get; }
        private IShaderEffect ShaderEffect {  get; }
        private CancellationTokenSource _cancellationTokenSource;
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
               // SpotSet.IndicateMissingValues();
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case nameof(GeneralSettings.ShaderCanvasHeight):
                case nameof(GeneralSettings.ShaderCanvasWidth):
                
                    //case nameof(GeneralSettings.Shaderparam1):
                    //case nameof(GeneralSettings.Shaderparam2):
                    //case nameof(GeneralSettings.Shaderparam3):
                    //case nameof(GeneralSettings.Shaderparam4):
                    RefreshReadingState();
                    break;
            }
        }
        private void DevicePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case nameof(DeviceSettings.TransferActive):
                case nameof(DeviceSettings.SelectedEffect):
                case nameof(DeviceSettings.DeviceRectLeft):
                case nameof(DeviceSettings.DeviceRectTop):
                    //case nameof(DeviceSettings.DeviceCanvasX):
                    //case nameof(DeviceSettings.DeviceCanvasY):
                    //case nameof(DeviceSettings.DeviceSizeX):
                    //case nameof(DeviceSettings.DeviceSizeY):
                    RefreshReadingState();
            break;
            }
        }
        public void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(ShaderReader) + " is already running!");

            IsRunning = true;
            //NeededRefreshing = false;
            _log.Debug("Started Shader Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
          

            try
            {



                while (!token.IsCancellationRequested)
                {
                    //get bitmap image of each frame
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetShaderFrame(image));
                    var width = DeviceSettings.DeviceRectWidth;
                    var height = DeviceSettings.DeviceRectHeight;
                    var x = DeviceSettings.DeviceRectLeft;
                    var y = DeviceSettings.DeviceRectTop;
                    // TraceFrameDetails(newImage);

                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;

                 
                    
                    image.LockBits(new Rectangle(x, y, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb, bitmapData);

                    lock (DeviceSpotSet.Lock)
                    {
                       var useLinearLighting = GeneralSettings.UseLinearLighting == 0;

          

                     
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
                                    , GeneralSettings.SaturationTreshold, spot.Red, spot.Green, spot.Blue);

                            var spotColor = new OpenRGB.NET.Models.Color(finalR, finalG, finalB);

                            var semifinalSpotColor = Brightness.applyBrightness(spotColor, 100);
                            ApplySmoothing(semifinalSpotColor.R, semifinalSpotColor.G, semifinalSpotColor.B
                                    , out byte RealfinalR, out byte RealfinalG, out byte RealfinalB,
                                 spot.Red, spot.Green, spot.Blue);
                            spot.SetColor(RealfinalR, RealfinalG, RealfinalB, true);
                         

                                });
                       



                    }



                    image.UnlockBits(bitmapData);

                    int minFrameTimeInMs = 1000 / GeneralSettings.LimitFps;
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
                //_desktopDuplicator?.Dispose();
               // _desktopDuplicator = null;
                _log.Debug("Stopped Shader Reader.");
                IsRunning = false;
                GC.Collect();
            }
            

            //foreach spot in spotset
            // get color of each spot by averaging color of rectangular region created by each spot
            // set spot color
            // smooth out color if needed

            // garbage collection, catch exception..

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
            r *= GeneralSettings.WhitebalanceRed / 100f;
            g *= GeneralSettings.WhitebalanceGreen / 100f;
            b *= GeneralSettings.WhitebalanceBlue / 100f;

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
            int smoothingFactor = GeneralSettings.SmoothFactor;


            semifinalR = (byte)((r + smoothingFactor * lastColorR) / (smoothingFactor + 1));
            semifinalG = (byte)((g + smoothingFactor * lastColorG) / (smoothingFactor + 1));
            semifinalB = (byte)((b + smoothingFactor * lastColorB) / (smoothingFactor + 1));
        }

        [Obsolete]
        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }
        
        private Bitmap GetShaderFrame(Bitmap reusableImage)
        {
            Bitmap ShaderBitmap;
            if(reusableImage!=null)
            ShaderBitmap = reusableImage;
            else
            ShaderBitmap = new Bitmap(240, 135, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var ShaderBitmapData = ShaderBitmap.LockBits(new Rectangle(0, 0, 240, 135), ImageLockMode.WriteOnly, ShaderBitmap.PixelFormat);
            IntPtr pixelAddress = ShaderBitmapData.Scan0;
            
                var CurrentFrame = ShaderEffect.Frame;
                if(CurrentFrame == null)
                {
                    return null;
                }
                else
                {
                Marshal.Copy(CurrentFrame, 0, pixelAddress, CurrentFrame.Length);
                ShaderBitmap.UnlockBits(ShaderBitmapData);
                return ShaderBitmap;

            }
               
        

        }
       

        private byte FadeNonLinear(float color)
        {
            var cacheIndex = (int)(color * 10);
            return _nonLinearFadingCache[Math.Min(2560 - 1, Math.Max(0, cacheIndex))];
        }
        private readonly byte[] _nonLinearFadingCache = Enumerable.Range(0, 2560)
           .Select(n => FadeNonLinearUncached(n / 10f))
           .ToArray();

     
        private static byte FadeNonLinearUncached(float color)
        {
            const float factor = 80f;
            return (byte)(256f * ((float)Math.Pow(factor, color / 256f) - 1f) / (factor - 1));
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
        public void Stop() // stop reading shader
        {
            //cancel running
         

        }

       

        public void RefreshReadingState()
        {    // create new thread with cancelation token and run base on isrunning and Shouldberunning
             //update if parametter changed (position, size...)

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = DeviceSettings.TransferActive && DeviceSettings.SelectedEffect == 5;

            //  var shouldBeRefreshing = NeededRefreshing;



            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Shader Reading for device Named " + DeviceSettings.DeviceName);
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Shader Reading for device Named " + DeviceSettings.DeviceName);
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "ShaderReader"
                };
                thread.Start();


            }
        }

    }
}
