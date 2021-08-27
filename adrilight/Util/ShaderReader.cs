using adrilight.Spots;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace adrilight.Util
{
    internal class ShaderReader :IShaderReader
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        public ShaderReader(IGeneralSettings generalSettings, IDeviceSpotSet deviceSpotSet, int graphicAdapter, int output, IDeviceSettings deviceSettings)
        {


            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            DeviceSpotSet = deviceSpotSet ?? throw new ArgumentNullException(nameof(deviceSpotSet));
            DeviceSettings = deviceSettings ?? throw new ArgumentNullException(nameof(deviceSettings));

            GeneralSettings.PropertyChanged += PropertyChanged;

         
            RefreshReadingState();

            _log.Info($"DesktopDuplicatorReader created.");
        }
        public bool IsRunning { get; set; }
        private IGeneralSettings GeneralSettings { get; }
        private IDeviceSettings DeviceSettings { get; }
        private IDeviceSpotSet DeviceSpotSet { get; }

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

            //get bitmap image of each frame

            //foreach spot in spotset
              // get color of each spot by averaging color of rectangular region created by each spot
              // set spot color
              // smooth out color if needed

            // garbage collection, catch exception..

        }
        public void Stop() // stop reading shader
        {
            //cancel running

        }
        public void RefreshReadingState()
        {    // create new thread with cancelation token and run base on isrunning and Shouldberunning
            //update if parametter changed (position, size...)
        }

    }
}
