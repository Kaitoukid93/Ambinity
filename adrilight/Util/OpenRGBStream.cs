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

namespace adrilight
{
    internal sealed class
        OpenRGBStream : IDisposable, ISerialStream
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public OpenRGBStream(IDeviceSettings deviceSettings,IOpenRGBClientDevice openRGBDevice, IDeviceSpotSet deviceSpotSet, IGeneralSettings generalSettings)
        {
            GeneralSettings = generalSettings ?? throw new ArgumentException(nameof(generalSettings));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            OpenRGBClientDevice = openRGBDevice ?? throw new ArgumentNullException(nameof(openRGBDevice));
            DeviceSettings.PropertyChanged += UserSettings_PropertyChanged;
            RefreshTransferState();

            _log.Info($"SerialStream created.");

            
        }
        //Dependency Injection//
        private IDeviceSettings DeviceSettings { get; set; }
        private IGeneralSettings GeneralSettings { get; set; }
        private IDeviceSpotSet DeviceSpotSet { get; set; }
        private int deviceID = 0;
         
        private bool CheckOpenRGBClientStatus(string ip, int port)
        {
            
            bool isvalid = false;
            try
            {


                var client = new OpenRGBClient(ip, port, name: "Ambinity", autoconnect: true, timeout: 1000);

                var deviceCount = client.GetControllerCount();
                var devices = client.GetAllControllerData();
              


                for(int i= 0; i < deviceCount; i++)
                {
                    if (devices[i].Serial == DeviceSettings.DeviceSerial);//found out desire device at pos i
                    deviceID = i;
                    isvalid = true;

                }
            }
            catch (TimeoutException)
            {
                HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
            }
            return isvalid;
        }
        private void UserSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeviceSettings.TransferActive):
               // case nameof(DeviceSettings.DevicePort):
                    RefreshTransferState();
                    break;
            }
        }

        public bool IsValid() => CheckOpenRGBClientStatus("127.0.0.1",6742);
        //public bool IsAcess() => !BlockedComport.Contains(UserSettings.ComPort);
        //public IList<string> BlockedComport = new List<string>();

        private void RefreshTransferState()
        {

            if (DeviceSettings.TransferActive)
            {
                if (IsValid() /*&& CheckSerialPort(DeviceSettings.DevicePort)*/)
                {

                    //start it
                    _log.Debug("starting the OpenRGBstream for device Name : " + DeviceSettings.DeviceName);
                    Start();
                }
               
            }

            else if (!DeviceSettings.TransferActive && IsRunning)
            {
                //stop it
                _log.Debug("stopping the serial stream");
                Stop();
            }
        }

        private readonly byte[] _messagePreamble = { (byte)'a', (byte)'b', (byte)'n' };
        private readonly byte[] _messagePostamble = { 15, 12, 93 };
        private readonly byte[] _messageZoeamble = { 15, 12, 93 };
        private readonly byte[] _commandmessage = { 15, 12, 93 };



        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;


        private int frameCounter;
        private int blackFrameCounter;
        private int _iD;
        public int ID {
            get { return DeviceSettings.DeviceID; }
            set
            {
                _iD = value;
            }
        }
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

        public bool IsRunning => _workerThread != null && _workerThread.IsAlive;



        public IOpenRGBClientDevice OpenRGBClientDevice { get; set; }



        private OpenRGB.NET.Models.Color[] GetOutputStream()
        {
            OpenRGB.NET.Models.Color[] outputColor = new OpenRGB.NET.Models.Color[DeviceSettings.NumLED];



            lock (DeviceSpotSet.Lock)
            {
                int counter = 0;
                foreach (DeviceSpot spot in DeviceSpotSet.Spots)
                {
                    var color = new OpenRGB.NET.Models.Color(spot.Red, spot.Green, spot.Blue);
                    outputColor[counter] = color;
                   

                    counter++;

                }

                return outputColor;

            }
        }
      

        private void DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                    try
                    {



                        var client = new OpenRGBClient(name: "My OpenRGB Client", autoconnect: true, timeout: 1000);

                        var deviceCount = client.GetControllerCount();
                        var devices = client.GetAllControllerData();
                    

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var outputColor = GetOutputStream();
                           
                                
                                    client.UpdateLeds(deviceID, outputColor);

                                

                            Thread.Sleep(18);
                        }
                    }
                    catch (TimeoutException)
                    {
                        HandyControl.Controls.MessageBox.Show("OpenRGB server Không khả dụng, hãy start server trong app OpenRGB (SDK Server)");
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












