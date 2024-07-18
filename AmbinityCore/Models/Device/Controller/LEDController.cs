using AmbinityCore.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device;

public class LEDController : ObservableObject, ILEDController
{
    private string _deviceName = "New Device";

    /// <summary>
    /// Display name of the device
    /// </summary>
    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    private string _deviceDescription = "";

    /// <summary>
    /// Describe the function and look of the device
    /// </summary>
    public string DeviceDescription
    {
        get => _deviceDescription;
        set => SetProperty(ref _deviceDescription, value);
    }

    private DeviceType _deviceType = Enums.DeviceType.Unknown;

    /// <summary>
    /// device catergory
    /// </summary>
    public DeviceType DeviceType
    {
        get => _deviceType;
        set => SetProperty(ref _deviceType, value);
    }

    private bool _isTransferEnabled = false;

    /// <summary>
    /// enable or disable data transfer to the device
    /// </summary>
    [JsonIgnore]
    public bool IsTransferEnabled
    {
        get => _isTransferEnabled;
        set => SetProperty(ref _isTransferEnabled, value);
    }

    private ILEDControllerHardwareSettings _iledControllerHardwareSettings;

    public ILEDControllerHardwareSettings IledControllerHardwareSettings
    {
        get => _iledControllerHardwareSettings;
        set => SetProperty(ref _iledControllerHardwareSettings, value);
    }

    private List<AmbinityDevice> _slaveDevices;
    public List<AmbinityDevice> SlaveDevices
    {
        get => _slaveDevices;
        set => SetProperty(ref _slaveDevices, value);
    }
}