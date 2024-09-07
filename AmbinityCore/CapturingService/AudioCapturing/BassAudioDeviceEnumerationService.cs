
using AmbinityCore.Models.Lighting.Zone.Configuration;
using ManagedBass.Wasapi;
using Serilog;


namespace AmbinityCore.CapturingService.AudioCapturing;

public class BassAudioDeviceEnumerationService : IDisposable
{
    #region Properties & Fields
    

    #endregion

    #region Constructors

    public BassAudioDeviceEnumerationService()
    {
      
        Log.Verbose("Audio device enumerator service created.");
        Log.Verbose("Audio device event interface registered.");
    }

    #endregion

    #region Methods
    

    public List<AudioDevice> GetAvailableAudioDevices()
    {
        var availableDevices = new List<AudioDevice>();
        var defaultDevice = new AudioDevice();
        int devicecount = BassWasapi.DeviceCount;
        var defaultBassID = -1;
        string defaultID = "";
        for (int i = 1; i < devicecount; i++)
        {
            var device = BassWasapi.GetDeviceInfo(i);
            if (device.IsDefault && device.IsEnabled)
            {
                defaultID = device.ID;
            }

            if (device.IsEnabled && device.IsLoopback)
            {
                var audioDevice = new AudioDevice() { Name = device.Name, ID = i };
                if (device.ID == defaultID)
                {
                    audioDevice.Name += " [default]";
                }
                availableDevices.Add(audioDevice);
            }
        }

        return (availableDevices);
    }

    public AudioDevice GetDefaultAudioDevice()
    {
        var defaultDevice = new AudioDevice();
        int devicecount = BassWasapi.DeviceCount;
        var defaultBassID = -1;
        string defaultID = "";
        for (int i = 1; i < devicecount; i++)
        {
            var device = BassWasapi.GetDeviceInfo(i);
            if (device.IsDefault && device.IsEnabled)
            {
                defaultID = device.ID;
            }

            if (device.IsEnabled && device.IsLoopback)
            {
                if (device.ID == defaultID)
                {
                    defaultDevice = new AudioDevice() { Name = device.Name, ID = i };
                    defaultDevice.Name += " [default]";
                   break;
                }
            }
        }

        return defaultDevice;
    }
    public void Dispose()
    {

    }

    #endregion
}