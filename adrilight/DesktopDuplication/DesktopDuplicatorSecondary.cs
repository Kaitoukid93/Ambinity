using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using adrilight.Extensions;

using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = SharpDX.Mathematics.Interop.RawRectangle;
using adrilight.Util;
using System.Windows;
using adrilight.DesktopDuplication;
using System.Threading;
using NLog.Fluent;
using Castle.Core.Logging;
using Polly;
using NLog;
using adrilight.ViewModel;
using GalaSoft.MvvmLight;

namespace adrilight
{
    /// <summary>
    /// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
    /// </summary>
    internal class DesktopDuplicatorSecondary : ViewModelBase, IDisposable , IDesktopDuplicatorSecondary
    {
        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly Device _device;
        private OutputDescription _outputDescription;
        private readonly OutputDuplication _outputDuplication;

        private Texture2D _stagingTexture;
        private Texture2D _smallerTexture;
        private ShaderResourceView _smallerTextureView;

        /// <summary>
        /// Duplicates the output of the specified monitor on the specified graphics adapter.
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
        /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicatorSecondary(IGeneralSettings userSettings)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            //MainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
           // mainView.PropertyChanged += PropertyChanged;
            userSettings.PropertyChanged += PropertyChanged;
            var whichGraphicsCardAdapter = 0;
            var whichOutputDevice = 1;
            Adapter1 adapter;
            if(UserSettings.ShouldbeRunningSecondary)
            {
                try
                {
                    adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
                }
                catch (SharpDXException ex)
                {
                    throw new DesktopDuplicationException("Could not find the specified graphics card adapter.", ex);
                }
                _device = new Device(adapter);
                Output output;
                try
                {
                    output = adapter.GetOutput(whichOutputDevice);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.NotFound)
                    {

                        HandyControl.Controls.MessageBox.Show(" Không thể capture màn hình " + (whichOutputDevice + 1).ToString(), "Screen Capture", MessageBoxButton.OK, MessageBoxImage.Warning);
                        output = adapter.GetOutput(0);

                    }
                    else
                    {
                        throw new DesktopDuplicationException("Unknown Device Error", ex);
                    }






                }
                var output1 = output.QueryInterface<Output1>();
                _outputDescription = output.Description;

                try
                {
                    _outputDuplication = output1.DuplicateOutput(_device);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                    {
                        throw new DesktopDuplicationException(
                            "There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                    }
                    else if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessDenied.Result.Code)
                    {
                        //Dispose();
                        throw new DesktopDuplicationException("Access Denied");
                    }
                    else
                    {
                        Dispose();
                        GC.Collect();
                        //retry right here??
                        throw new Exception("Unknown, just retry");



                    }


                }
            }    
           
           
            _retryPolicy = Policy.Handle<Exception>()
               .WaitAndRetryForever(ProvideDelayDuration);
            _log.Info($"Desktop Duplicator created for Display 1.");

            RefreshCapturingState();
        }
        private readonly Policy _retryPolicy;
        private IGeneralSettings UserSettings { get; }
       // private MainViewViewModel MainView { get; }
        public bool IsRunning { get; private set; } = false;
        public byte[] DesktopFrame { get; set; }
        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {

                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming
                
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {

                case nameof(UserSettings.ShouldbeRunningSecondary):
             //  case nameof(MainView.IsSettingsWindowOpen):

                    RefreshCapturingState();
                    break;

                case nameof(UserSettings.SelectedDisplay):
                case nameof(UserSettings.SelectedAdapter):
                    RefreshCaptureSource();
                    break;
            }
        }
        public void RefreshCaptureSource()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = UserSettings.ShouldbeRunningSecondary;
            //  var shouldBeRefreshing = NeededRefreshing;
            if (isRunning && shouldBeRunning)
            {
                //start it

                IsRunning = false;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                _log.Debug("starting DesktopDuplicator on display 1");
                _cancellationTokenSource = new CancellationTokenSource();
                _workerThread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicator"
                };
                _workerThread.Start();

            }
        }
        public void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = UserSettings.ShouldbeRunningSecondary;
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
        private CancellationTokenSource _cancellationTokenSource;
        private Thread _workerThread;
        public void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(DesktopDuplicatorReader) + " is already running!");

            IsRunning = true;
           // NeededRefreshing = false;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            BitmapData bitmapData = new BitmapData();
       

            try
            {



                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    var newImage = _retryPolicy.Execute(() => GetLatestFrame(image));
                   // TraceFrameDetails(newImage);

                    if (newImage == null)
                    {
                        //there was a timeout before there was the next frame, simply retry!
                        continue;
                    }
                    image = newImage;
                  
                    // Lock the bitmap's bits.  
                    var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
                    System.Drawing.Imaging.BitmapData bmpData =
                        image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                        image.PixelFormat);

                    // Get the address of the first line.
                    IntPtr ptr = bmpData.Scan0;

                    // Declare an array to hold the bytes of the bitmap.
                    int bytes = Math.Abs(bmpData.Stride) * image.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                    DesktopFrame = rgbValues;
                    RaisePropertyChanged(nameof(DesktopFrame));
                    // if(MainView.IsSettingsWindowOpen)
                    // MainView.SetPreviewImage(DesktopFrame);

                    //bool isPreviewRunning = SettingsViewModel.IsSettingsWindowOpen && SettingsViewModel.IsPreviewTabOpen;
                    //if (isPreviewRunning)
                    //{
                    //   MainViewViewModel.SetPreviewImage(image);



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
               
                _log.Debug("Stopped Desktop Duplicator on display 1.");
                IsRunning = false;
                GC.Collect();
            }
        }

        private static readonly FpsLogger _desktopFrameLogger = new FpsLogger("DesktopDuplication");


        /// <summary>
        /// Retrieves the latest desktop image and associated metadata.
        /// </summary>
        public Bitmap GetLatestFrame(Bitmap reusableImage)
        {
            // Try to get the latest frame; this may timeout
            var succeeded = RetrieveFrame();
            if (!succeeded)
                return null;

            _desktopFrameLogger.TrackSingleFrame();

            return ProcessFrame(reusableImage);

        }
      

        private const int mipMapLevel = 3;
        private const int scalingFactor = 1 << mipMapLevel;

        private bool RetrieveFrame()
        {

            var desktopWidth = _outputDescription.DesktopBounds.GetWidth();
            var desktopHeight = _outputDescription.DesktopBounds.GetHeight();

            if (_stagingTexture == null)
            {
                _stagingTexture = new Texture2D(_device, new Texture2DDescription() {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = desktopWidth / scalingFactor,
                    Height = desktopHeight / scalingFactor,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging // << can be read by CPU
                });
            }
            SharpDX.DXGI.Resource desktopResource;
            try
            {
                if (_outputDuplication == null) throw new Exception("_outputDuplication is null");
                _outputDuplication.TryAcquireNextFrame(1000, out var frameInformation, out desktopResource);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    return false;
                }
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                {
                    // ReleaseFrame();
                    throw new Exception("Access Lost, resolution might be changed");
                    //do something to restart desktop duplicator here


                }
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.InvalidCall.Result.Code)
                {
                    // ReleaseFrame();
                    throw new Exception("Invalid call, might be retrying");


                }

                throw new DesktopDuplicationException("Failed to acquire next frame.", ex);
            }
            if (desktopResource == null) throw new Exception("desktopResource is null");

            if (_smallerTexture == null)
            {
                _smallerTexture = new Texture2D(_device, new Texture2DDescription {
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = desktopWidth,
                    Height = desktopHeight,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                    MipLevels = mipMapLevel + 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default
                });
                _smallerTextureView = new ShaderResourceView(_device, _smallerTexture);
            }


            using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
            {
                if (_device == null) throw new Exception("_device is null");
                //if (_device.ImmediateContext == null) throw new Exception("_device.ImmediateContext is null");                

                _device.ImmediateContext.CopySubresourceRegion(tempTexture, 0, null, _smallerTexture, 0);
            }
            //_outputDuplication.ReleaseFrame();
            if (_outputDuplication != null)
                _outputDuplication.ReleaseFrame();

            // Generates the mipmap of the screen
            _device.ImmediateContext.GenerateMips(_smallerTextureView);

            // Copy the mipmap 1 of smallerTexture (size/2) to the staging texture
            _device.ImmediateContext.CopySubresourceRegion(_smallerTexture, mipMapLevel, null, _stagingTexture, 0);

            desktopResource.Dispose(); //perf?
            return true;
        }

        private Bitmap ProcessFrame(Bitmap reusableImage)
        {
            // Get the desktop capture texture
            var mapSource = _device.ImmediateContext.MapSubresource(_stagingTexture, 0, MapMode.Read, MapFlags.None);

            Bitmap image;
            var width = _outputDescription.DesktopBounds.GetWidth() / scalingFactor;
            var height = _outputDescription.DesktopBounds.GetHeight() / scalingFactor;


            if (reusableImage != null && reusableImage.Width == width && reusableImage.Height == height)
            {
                image = reusableImage;
            }
            else
            {
                image = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            }

            var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);

            // Copy pixels from screen capture Texture to GDI bitmap

            var mapDest = image.LockBits(boundsRect, ImageLockMode.WriteOnly, image.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;

            if (mapSource.RowPitch == mapDest.Stride)
            {
                //fast copy
                Utilities.CopyMemory(destPtr, sourcePtr, height * mapDest.Stride);
            }
            else
            {
                //safe copy
                for (int y = 0; y < height; y++)
                {
                    // Copy a single line 
                    Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }
            }

            // Release source and dest locks
            image.UnlockBits(mapDest);
            _device.ImmediateContext.UnmapSubresource(_stagingTexture, 0);

            return image;
        }


        public bool IsDisposed { get; private set; }

        public static int ScalingFactor => scalingFactor;

        public void Dispose()
        {
            IsDisposed = true;
            _smallerTexture?.Dispose();
            _smallerTextureView?.Dispose();
            _stagingTexture?.Dispose();
            _outputDuplication?.Dispose();
            _device?.Dispose();
            // _desktopFrameLogger?.Dispose();
            GC.Collect();
        }
        private void ReleaseFrame()
        {
            try
            {
                _outputDuplication.ReleaseFrame();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to release frame.");
                }
            }
        }
    }
}