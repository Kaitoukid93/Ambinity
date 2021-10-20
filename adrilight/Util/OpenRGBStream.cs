using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using NLog;
using System.Buffers;
using adrilight.Util;
using System.Linq;
using Newtonsoft.Json;
using System.Windows;
using adrilight.Spots;
using OpenRGB.NET;
using System.Collections.Generic;

namespace adrilight
{
    internal sealed class
        OpenRGBStream : IDisposable, IOpenRGBStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public OpenRGBStream(IDeviceSettings[] deviceSettings,IOpenRGBClientDevice openRGBDevice, IDeviceSpotSet[] deviceSpotSet, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));

            OpenRGBClientDevice = openRGBDevice ?? throw new ArgumentNullException(nameof(openRGBDevice));
            //DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            RefreshTransferState();

            _log.Info($"SerialStream created.");

            
        }
        //Dependency Injection//
        private IDeviceSettings[] DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSpotSet[] DeviceSpotSet { get; set; }
        
        //private int deviceID = 0;
        
         
       
        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(GeneralSettings.IsOpenRGBEnabled):
                    
                    RefreshTransferState();
                    break;
            }
        }

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;

        private void RefreshTransferState()
        {
           
            if(GeneralSettings.IsOpenRGBEnabled)
            {
                OpenRGBClientDevice.RefreshOpenRGBDeviceState();
                if (OpenRGBClientDevice.IsAvailable)
                {

                    //start it
                    _log.Debug("starting the OpenRGBstream");
                    foreach (var device in DeviceSettings)
                    {
                        if (device.DeviceSerial != "151293")
                            device.IsConnected = true;
                    }
                    Start();
                }
                else
                {
                    foreach (var device in DeviceSettings)
                    {
                        if (device.DeviceSerial != "151293")
                            device.IsConnected = false;
                    }
                }
            }

            else if (!GeneralSettings.IsOpenRGBEnabled && IsRunning)
            {
                //stop it
                _log.Debug("stopping the serial stream");
                Stop();
            }


        }

  



        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;

        public void DFU()
        {

        }

        public void Start()
        {
            _log.Debug("Start called.");
            if (_workerThread != null) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(DoWork) {
                Name = "Serial sending",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            WinApi.TimeBeginPeriod(1);

            // The call has failed

            _workerThread.Start(_cancellationTokenSource.Token);
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

     



        public IOpenRGBClientDevice OpenRGBClientDevice { get; set; }



        private List<OpenRGB.NET.Models.Color[]>  GetOutputStream()
        {
            OpenRGB.NET.Models.Color[] blankColor = null;
            List<OpenRGB.NET.Models.Color[]> outputColor = new List<OpenRGB.NET.Models.Color[]>();
            var client = OpenRGBClientDevice.AmbinityClient;
            if (client != null)
            {
                var devices = client.GetAllControllerData();
                outputColor.AddRange(Enumerable.Repeat(blankColor, devices.Length));
                for (var i=0; i < devices.Length; i++)
                {
                    foreach (var spotSet in DeviceSpotSet)
                    {
                        if(spotSet.DeviceLocation==devices[i].Location)
                        {
                            var colors = new OpenRGB.NET.Models.Color[spotSet.Spots.Length];
                            lock (spotSet.Lock)
                            {
                                int counter = 0;
                                foreach (DeviceSpot spot in spotSet.Spots)
                                {
                                    var color = new OpenRGB.NET.Models.Color(spot.Red, spot.Green, spot.Blue);
                                    colors[counter++] = color;

                                }
                                outputColor.Insert(i, colors);


                            }
                        }
                     
                    }
                }
              
            }
         
            return outputColor;

        }
      

        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                    try
                    {
                    var client = OpenRGBClientDevice.AmbinityClient;
                    var devices = client.GetAllControllerData();
                    if(client.Connected)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var outputColor = GetOutputStream();
                            for(int i=0;i< outputColor.Count; i++)
                            {
                                if(outputColor[i]!=null)
                                client.UpdateLeds(i, outputColor[i]);
                            }

                            Thread.Sleep(15);
                        }
                    }
                      
                    }
                    catch (TimeoutException)
                    {
                        HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                    foreach(var device in DeviceSettings)
                    {
                        if(device.DeviceSerial!="151293")
                        device.IsConnected=false; 
                    }
                    Stop();
                    }
                catch ( Exception ex)
                {
                    HandyControl.Controls.MessageBox.Show(ex.ToString());
                    foreach (var device in DeviceSettings)
                    {
                        if (device.DeviceSerial != "151293")
                            device.IsConnected = false;
                    }
                    Stop();
                }
                     





                        
                    }
                }
              
            

        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }
    }
}












