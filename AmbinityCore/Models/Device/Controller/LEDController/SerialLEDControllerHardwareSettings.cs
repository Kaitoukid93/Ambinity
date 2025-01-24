using System.Text.Json.Serialization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device;

public class SerialLEDControllerHardwareSettings : ObservableObject, ILEDControllerHardwareSettings
{
    private string _deviceName = "Serial Device";

    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }
    private string _deviceSerialNumber = "00000";

    public string DeviceSerialNumber
    {
        get => _deviceSerialNumber;
        set => SetProperty(ref _deviceSerialNumber, value);
    }

    private string _deviceManufacturer = "Unknown";

    /// <summary>
    /// Manufacturer of the device, this represents the brand name such as "Ambino", "Corsair"
    /// </summary>
    public string DeviceManufacturer
    {
        get => _deviceManufacturer;
        set => SetProperty(ref _deviceManufacturer, value);
    }

    private string _deviceFirmwareVersion = "Unknown";

    /// <summary>
    /// The firmware version read from device, only available if the device respond tho the hand-shake protocol
    /// </summary>
    public string DeviceFirmwareVersion
    {
        get => _deviceFirmwareVersion;
        set => SetProperty(ref _deviceFirmwareVersion, value);
    }

    private string _deviceHardwareVersion = "Unknown";

    /// <summary>
    /// The hardware version read from device, only available if the device respond tho the hand-shake protocol
    /// </summary>
    public string DeviceHardwareVersion
    {
        get => _deviceHardwareVersion;
        set => SetProperty(ref _deviceHardwareVersion, value);
    }
    private HardwareTypeEnum _hardwareType = HardwareTypeEnum.Unknown;

    /// <summary>
    /// Hardware type inherit from parent controller
    /// </summary>
    public HardwareTypeEnum HardwareType
    {
        get => _hardwareType;
        set => SetProperty(ref _hardwareType, value);
    }

    /// <summary>
    /// The production date read from device, only available if the device respond tho the hand-shake protocol
    /// </summary>
    private string _deviceProductionDate = "Unknown";

    public string DeviceProductionDate
    {
        get => _deviceProductionDate;
        set => SetProperty(ref _deviceProductionDate, value);
    }

    private string _deviceCommunicationAddress = "COM1";
    
    /// <summary>
    /// The COM port using to send data out
    /// </summary>
    [JsonIgnore]
    public string DeviceCommunicationAddress
    {
        get => _deviceCommunicationAddress;
        set => SetProperty(ref _deviceCommunicationAddress, value);
    }

    

    #region Hardware Lighting Settings
    public bool HWL_enable { get; set; }
    private bool _statusLEDEnable = true;
    private byte _hwl_returnafter = 3;
    private byte _hwl_effectMode = 0;
    private byte _hwl_effectSpeed = 10;
    private byte _hwl_brightness = 50;
    private Color _hwl_singleColor = Avalonia.Media.Colors.Aqua;
    private Color[] _hwl_palette;
    private byte _hwl_effectIntensity = 16;
    private byte _hwl_version = 0;
    private byte _hwl_MaxLEDPerOutput = 80;

    /// <summary>
    /// Enable Status LED (if available)
    /// </summary>
    public bool StatusLEDEnable
    {
        get => _statusLEDEnable;
        set => SetProperty(ref _statusLEDEnable, value);
    }

    public byte HWL_returnafter
    {
        get => _hwl_returnafter;
        set => SetProperty(ref _hwl_returnafter, value);
    }

    public byte HWL_effectMode
    {
        get => _hwl_effectMode;
        set => SetProperty(ref _hwl_effectMode, value);
    }

    public byte HWL_effectSpeed
    {
        get => _hwl_effectSpeed;
        set => SetProperty(ref _hwl_effectSpeed, value);
    }

    public byte HWL_effectIntensity
    {
        get => _hwl_effectIntensity;
        set => SetProperty(ref _hwl_effectIntensity, value);
    }

    public byte HWL_brightness
    {
        get => _hwl_brightness;
        set => SetProperty(ref _hwl_brightness, value);
    }

    public Color HWL_singleColor
    {
        get => _hwl_singleColor;
        set => SetProperty(ref _hwl_singleColor, value);
    }

    public Color[] HWL_palette
    {
        get => _hwl_palette;
        set => SetProperty(ref _hwl_palette, value);
    }

    public byte HWL_version
    {
        get => _hwl_version;
        set => SetProperty(ref _hwl_version, value);
    }

    public byte HWL_MaxLEDPerOutput
    {
        get => _hwl_MaxLEDPerOutput;
        set => SetProperty(ref _hwl_MaxLEDPerOutput, value);
    }

  

    #endregion
}