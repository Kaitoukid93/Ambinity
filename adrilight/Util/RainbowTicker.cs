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
    internal class RainbowTicker : IRainbowTicker
    {
       

        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();

        public RainbowTicker( IDeviceSettings[] allDeviceSettings, IGeneralSettings generalSettings)
        {
            //DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            AllDeviceSettings = allDeviceSettings ?? throw new ArgumentNullException(nameof(allDeviceSettings));
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            //DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            //AllDeviceSpotSet = allDeviceSpotSet ?? throw new ArgumentNullException(nameof(allDeviceSpotSet));
           
            
          
           GeneralSettings.PropertyChanged += PropertyChanged;
            
           
            RefreshColorState();

            _log.Info($"RainbowColor Created");

        }
        //Dependency Injection//
       // private IDeviceSettings DeviceSettings { get; }
       // private IDeviceSettings ParrentDevice { get; }
        private IDeviceSettings[] AllDeviceSettings { get; }
        private IDeviceSpotSet[] AllDeviceSpotSet { get; }
        private IGeneralSettings GeneralSettings { get; }
        private IDeviceSpotSet DeviceSpotSet { get; }
        private double _startIndex;
     
        public double StartIndex {
            get {  return _startIndex; }    
            set {  _startIndex = value; }
        }

      
           
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;

        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeneralSettings.SystemRainbowSpeed):
               
                

                    RefreshColorState();
                    break;
            }
        }
        //private void ParrentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case nameof(ParrentDevice.SyncOn):
        //            RefreshColorState();
        //            break;
        //    }
        //}
        
        private void RefreshColorState()
        {

            var isRunning = _cancellationTokenSource != null && IsRunning; // Check if sync mode is enabled
          
                 var shouldBeRunning = true;
          
            
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the Rainbow Ticker");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else if (!isRunning && shouldBeRunning)
            {
                //start it
                _log.Debug("starting the Rainbow Ticker");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "RainbowTicker"
                };
                thread.Start();
            }
        }




       
        public void Run(CancellationToken token)

        {
            

            if (IsRunning) throw new Exception(" Rainbow Ticker is already running!");

            IsRunning = true;

            _log.Debug("Started Rainbow Ticker.");


            try
            {
                

                while (!token.IsCancellationRequested)
                {
                    double speed = GeneralSettings.SystemRainbowSpeed / 5d;
                    StartIndex += speed;
                            if (StartIndex > 450)
                            {
                            StartIndex = 0;
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

                _log.Debug("Stopped Rainbow Ticking.");
                IsRunning = false;
            }


        }



    }
}
