using System;
using System.Windows.Media.Imaging;
using adrilight.Util;
using System.Threading;
using NLog;
using adrilight.Spots;
using ComputeSharp;
using adrilight.Shaders;
using System.Runtime.InteropServices;
using System.Windows;
using adrilight.ViewModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Diagnostics;

namespace adrilight
{
    internal class ShaderEffect : ViewModelBase, IShaderEffect
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public ShaderEffect(IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));      
            GeneralSettings.PropertyChanged += PropertyChanged;
            RefreshColorState();
            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        private IGeneralSettings GeneralSettings { get; }
       

        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        public  WriteableBitmap MatrixBitmap { get; set; }
        public byte[] Frame { get; set; }

        

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                
                case nameof(GeneralSettings.ShaderCanvasWidth):
                case nameof(GeneralSettings.ShaderCanvasHeight):
                case nameof(GeneralSettings.ShaderX):
                case nameof(GeneralSettings.ShaderY):
                case nameof(GeneralSettings.SelectedShader):

                    RefreshColorState();
                    break;
            }
        }
        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true; /*DeviceSettings.TransferActive && DeviceSettings.SelectedEffect == 5;*/
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Shader Effect");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Shader Effect");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "ShaderEffectCreator"
                };
                thread.Start();
            }
        }





        public void Run(CancellationToken token)

        {
           
            if (IsRunning) throw new Exception(" Shader Effects is already running!");

            IsRunning = true;

            _log.Debug("Started Shader Effects Color.");


            try
            {
                using ReadWriteTexture2D<Bgra32, Float4> texture = Gpu.Default.AllocateReadWriteTexture2D<Bgra32, Float4>(240, 135);
               
                int frame = 0;
               
                Frame = null;
                

                while (!token.IsCancellationRequested)
                {



                    var frameTime = Stopwatch.StartNew();

                    // For each frame

                    float timestamp = 1 / 60f * frame;

                            // Run the shader
                           switch(GeneralSettings.SelectedShader)
                    {
                        case "Gooey":
                            Gpu.Default.ForEach(texture, new Gooey(timestamp));
                            break;
                        case "Fluid":
                            Gpu.Default.ForEach(texture, new Fluid(timestamp));
                            break;
                        case "Plasma":
                            Gpu.Default.ForEach(texture, new Plasma(timestamp));
                            break;
                        case "Falling":
                            Gpu.Default.ForEach(texture, new Falling(timestamp));
                            break;
                        case "MetaBalls":
                            Gpu.Default.ForEach(texture, new MetaBalls(timestamp));

                            break;
                        case "Pixel Rainbow":
                            Gpu.Default.ForEach(texture, new PixelRainbow(timestamp));

                            break;
                       


                    }


                    // Copy the rendered frame to a readback texture that can be accessed by the CPU.
                    // The rendered texture can only be accessed by the GPU, so we can't read from it.
                    var colorArray = new Bgra32[texture.Width * texture.Height];
                    var bitmapSpan = new Span<Bgra32>(colorArray);
                    // Access buffer.View here and do all your work with the frame data
                   
                    texture.CopyTo(bitmapSpan);
                   
                    var bytes = MemoryMarshal.Cast<Bgra32, byte>(bitmapSpan);
                    Frame = bytes.ToArray();    
                    RaisePropertyChanged(nameof(Frame));
                    int minFrameTimeInMs = 1000 / GeneralSettings.LimitFps;
                    var elapsedMs = (int)frameTime.ElapsedMilliseconds;
                    if (elapsedMs < minFrameTimeInMs)
                    {
                        Thread.Sleep(minFrameTimeInMs - elapsedMs);
                    }
                    frame++;

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


       

    }
}
