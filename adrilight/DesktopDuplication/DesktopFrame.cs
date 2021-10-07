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
using GalaSoft.MvvmLight;

namespace adrilight
{
    internal class DesktopFrame : ViewModelBase, IDesktopFrame
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();

        public DesktopFrame(IGeneralSettings userSettings)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
           
            
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryForever(ProvideDelayDuration);

            UserSettings.PropertyChanged += PropertyChanged;
            
            RefreshCapturingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }
        public byte[] Frame { get; set; }
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UserSettings.ShouldbeRunning):
               
                //case nameof(MainView.IsSettingsWindowOpen):
                //case nameof(MainView.IsPreviewTabOpen):
                    RefreshCapturingState();
                    break;
            }
        }

        //public RunStateEnum RunState { get; private set; } = RunStateEnum.Stopped;
        private CancellationTokenSource _cancellationTokenSource;


        private void RefreshCapturingState()
        {
            var isRunning = _cancellationTokenSource != null;
            var shouldBeRunning = UserSettings.ShouldbeRunning;

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
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "DesktopDuplicatorReader"
                };
                thread.Start();
            }
        }

        private IGeneralSettings UserSettings { get; }
        public bool IsRunning { get; set; }

      

        private readonly Policy _retryPolicy;

        private TimeSpan ProvideDelayDuration(int index)
        {
            if (index < 10)
            {
                //first second
                return TimeSpan.FromMilliseconds(100);
            }

            if (index < 10 + 256)
            {
                //steps where there is also led dimming
               
                return TimeSpan.FromMilliseconds(5000d / 256);
            }
            return TimeSpan.FromMilliseconds(1000);
        }

        private DesktopDuplicator _desktopDuplicator;

        public void Run(CancellationToken token)
        {
            if (IsRunning) throw new Exception(nameof(DesktopFrame) + " is already running!");

            IsRunning = true;
            _log.Debug("Started Desktop Duplication Reader.");
            Bitmap image = null;
            try
            {
                BitmapData bitmapData = new BitmapData();

                while (!token.IsCancellationRequested)
                {
                    var frameTime = Stopwatch.StartNew();
                    var context = new Context();
                    context.Add("image", image);
                    var newImage = _retryPolicy.Execute(c => GetNextFrame(c["image"] as Bitmap), context);
                    TraceFrameDetails(newImage);
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
                    Frame = rgbValues;
                    RaisePropertyChanged(nameof(Frame));
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

                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;

                _log.Debug("Stopped Desktop Duplication Reader.");

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

       

        private Bitmap GetNextFrame(Bitmap reusableBitmap)
        {
            if (_desktopDuplicator == null)
            {
                _desktopDuplicator = new DesktopDuplicator(0, 0);
            }

            try
            {
                return _desktopDuplicator.GetLatestFrame(reusableBitmap);
            }
            catch (Exception ex)
            {
                if (ex.Message != "_outputDuplication is null")
                {
                    _log.Error(ex, "GetNextFrame() failed.");
                }

                _desktopDuplicator?.Dispose();
                _desktopDuplicator = null;

                GC.Collect();

                throw;
            }
        }

     

    }
}