﻿using System;
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

namespace adrilight
{
    internal class ShaderEffect : ViewModelBase, IShaderEffect
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public ShaderEffect(IGeneralSettings generalSettings, IGeneralSpotSet generalSpotSet)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            GeneralSpotSet = generalSpotSet ?? throw new ArgumentNullException(nameof(generalSpotSet));
           // MainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
            //SettingsViewModel = settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));
            //Remove SettingsViewmodel from construction because now we pass SpotSet Dirrectly to MainViewViewModel
            GeneralSettings.PropertyChanged += PropertyChanged;
            // SettingsViewModel.PropertyChanged += PropertyChanged;
            RefreshColorState();
            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
        private IGeneralSettings GeneralSettings { get; }
        private IGeneralSpotSet GeneralSpotSet { get; }
       // private MainViewViewModel MainView { get; }

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
                using ReadWriteTexture2D<Rgba32, Float4> texture = Gpu.Default.AllocateReadWriteTexture2D<Rgba32, Float4>(240, 135);
                using ReadBackTexture2D<Rgba32> buffer = Gpu.Default.AllocateReadBackTexture2D<Rgba32>(240, 135);
                int frame = 0;
                var ColorArray = buffer.View.ToArray();
                Frame = null;
                Frame = new byte[ColorArray.Length * 4];

                while (!token.IsCancellationRequested)
                {
                 
                    

                       

                        // For each frame
                        
                            float timestamp = 1 / 60f * frame;

                            // Run the shader
                           Gpu.Default.ForEach(texture, new Gooey(timestamp));

                            // Copy the rendered frame to a readback texture that can be accessed by the CPU.
                            // The rendered texture can only be accessed by the GPU, so we can't read from it.
                            texture.CopyTo(buffer);

                    // Access buffer.View here and do all your work with the frame data
                    ColorArray = buffer.View.ToArray();

                    int index = 0;
                        for (var x=0;x <= ColorArray.GetUpperBound(0);x++)
                        {
                            for(var y=0;y <=ColorArray.GetUpperBound(1); y++)
                            {
                                Frame[index++] = ColorArray[x, y].B;
                                Frame[index++] = ColorArray[x, y].G;
                                Frame[index++] = ColorArray[x, y].R;
                                Frame[index++] = ColorArray[x, y].A;
                            
                            }
                        }
                    RaisePropertyChanged(nameof(Frame));
                    // if(MainView.IsSettingsWindowOpen)
                  //  MainView.SetPreviewImage(Frame);
                    Thread.Sleep(1000/60);
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
