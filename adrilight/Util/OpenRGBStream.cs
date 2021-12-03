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
using Polly;

namespace adrilight
{
    internal sealed class
        OpenRGBStream : IDisposable, IOpenRGBStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public OpenRGBStream(IDeviceSettings[] deviceSettings, IDeviceSpotSet[] deviceSpotSet, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
             _retryPolicy = Policy.Handle<Exception>().WaitAndRetry(retryCount: 10, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));

            //DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
           // IsInitialized = false;
            RefreshTransferState();

            _log.Info($"SerialStream created.");


        }
        //Dependency Injection//
        private IDeviceSettings[] DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSpotSet[] DeviceSpotSet { get; set; }
        private OpenRGB.NET.Models.Device[] AvailableDevices { get; set; }
      
        public bool IsInitialized { get; set; }
        private OpenRGBClient _ambinityClient;
        public OpenRGBClient AmbinityClient {
            get { return _ambinityClient; }
            set { _ambinityClient = value; }
        }

        //private int deviceID = 0;

        private readonly Policy _retryPolicy;

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

        public void RefreshTransferState()
        {
            

            if (GeneralSettings.IsOpenRGBEnabled&&!IsInitialized) // Only run OpenRGB Stream if User enable OpenRGB Utilities in General Settings
            {

                try
                {
                    if (AmbinityClient != null)
                        AmbinityClient.Dispose();
                    var attempt = 0;
                    _retryPolicy.Execute(() => RefreshOpenRGBDeviceState()); _log.Info($"Attempt {++attempt}"); 
                    //AmbinityClient = client;
                    if (AmbinityClient != null)
                    {
                        //var deviceCount = AmbinityClient.GetControllerCount();
                        var devices = AmbinityClient.GetAllControllerData();
                        //AvailableDevices = devices;
                        ////DeviceList = devices;
                        ////IsAvailable = true;
                        //_log.Debug("starting the OpenRGBstream");
                        foreach (var device in DeviceSettings)//check for exist device and push updates
                        {
                            if (device.DeviceSerial != "151293")
                                device.IsConnected = true;
                        }
                        foreach (var device in devices)
                        {
                            _log.Info($"Device found : " + device.Name.ToString());
                        }

                        Start();

                    }


                    //for (int i = 0; i < devices.Length; i++)
                    //{
                    //    var leds = Enumerable.Range(0, devices[i].Colors.Length)
                    //        .Select(_ => new Color(255, 0, 255))
                    //        .ToArray();
                    //    client.UpdateLeds(i, leds);
                    //}
                    IsInitialized = true;
                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                    IsInitialized = false;
                    //IsAvailable= false;

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
        public OpenRGBClient RefreshOpenRGBDeviceState()//init
        {
                 if(AmbinityClient!=null)
                AmbinityClient.Dispose();
                AmbinityClient = new OpenRGBClient("127.0.0.1", 6742, name: "Ambinity", autoconnect: true, timeout: 1000);
                return AmbinityClient;
        }
        public void DFU()
        {
            //nothing to do here with OpenRGB Devices
        }

        public void Start()
        {
            _log.Debug("Start called.");
               
            
            _cancellationTokenSource = new CancellationTokenSource();
            WinApi.TimeBeginPeriod(1);
            if(_workerThread!=null)
            {
                _workerThread = null;
            }
            
                _workerThread = new Thread(DoWork) {
                    Name = "Serial sending",
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };
                _workerThread.Start(_cancellationTokenSource.Token);
            

            

            // The call has failed


        }

        public void Stop()
        {
            _log.Debug("Stop called.");
            IsInitialized = false;
            if(AmbinityClient!=null)
            AmbinityClient.Dispose();
            if (_workerThread == null) return;
         
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _workerThread?.Join();
            _workerThread = null;
        }





        



        private List<OpenRGB.NET.Models.Color[]> GetOutputStream()
        {
            OpenRGB.NET.Models.Color[] blankColor = null;
            List<OpenRGB.NET.Models.Color[]> outputColor = new List<OpenRGB.NET.Models.Color[]>();
            var client = AmbinityClient;
            if (client != null)
            {
                var devices = client.GetAllControllerData();
                outputColor.AddRange(Enumerable.Repeat(blankColor, devices.Length));
                for (var i = 0; i < devices.Length; i++)
                {
                    foreach (var spotSet in DeviceSpotSet)
                    {
                        if (spotSet.DeviceLocation == devices[i].Location)
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
                    var client = AmbinityClient;
                  
                    if (client.Connected)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var outputColor = GetOutputStream();
                            for (int i = 0; i < client.GetAllControllerData().Length; i++)
                            {
                                if (outputColor[i] != null)
                                {
                                    client.SetMode(i, 1);
                                    client.UpdateLeds(i, outputColor[i]);
                                }
                                   
                            }

                            Thread.Sleep(10);
                        }
                    }

                }
                catch (TimeoutException)
                {
                    HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
                    foreach (var device in DeviceSettings)
                    {
                        if (device.DeviceSerial != "151293")
                            device.IsConnected = false;
                    }
                    Thread.Sleep(500);
                    Stop();
                }
                catch (Exception ex)
                {
                    //HandyControl.Controls.MessageBox.Show(ex.ToString());
                    //foreach (var device in DeviceSettings)
                    //{
                    //    if (device.DeviceSerial != "151293")
                    //        device.IsConnected = false;
                    //}   
                  
                    Thread.Sleep(500);
                    IsInitialized = false;
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = null;
                    Thread.Sleep(500);
                    RefreshTransferState();
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
