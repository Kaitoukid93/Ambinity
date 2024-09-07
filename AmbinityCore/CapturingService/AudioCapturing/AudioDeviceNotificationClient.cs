namespace AmbinityCore.CapturingService.AudioCapturing;

public class AudioDeviceNotificationClient
{
    public delegate void DefaultDeviceChangedHandler();
    public delegate void DeviceStateChangedHandler();
    public delegate void DevicePropertyChangedHandler();
    public event DefaultDeviceChangedHandler DefaultDeviceChanged;
    public event DeviceStateChangedHandler DeviceStateChanged;
    public event DevicePropertyChangedHandler DevicePropertyChanged;
    public event DevicePropertyChangedHandler DeviceCountChanged;
    public void OnDefaultDeviceChanged(int id)
    {
        if (DefaultDeviceChanged != null)
        {
            DefaultDeviceChanged();
        }
    }

    public void OnDeviceAdded(int id)
    {
        if (DeviceCountChanged != null)
        {
            DeviceCountChanged();
        }
    }
    public AudioDeviceNotificationClient()
    {
        if (Environment.OSVersion.Version.Major < 6)
        {
            throw new NotSupportedException("This functionality is only supported on Windows Vista or newer.");
        }
    }
    
}