using System.ComponentModel;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device.Controller;

public class SerialController : ObservableObject, IController
{
    /// <summary>
    /// bare-bones information from serial device
    /// </summary>
    public event Action? WorkingStateChanged;

    public event Action SerialPortChanged;
    public event Action TransferActiveChanged;

    private string resourcePath => Path.Combine(Constants.AppDataFolder, "Images");

    public SerialController()
    {
    }

    public HardwareTypeEnum HardwareType { get; set; }
    public bool AutoConnect { get; set; } = true;
    [JsonIgnore] public bool IsTransferActive { get; set; }
    private string _serialPort;

    public string SerialPort
    {
        get => _serialPort;
        set
        {
            _serialPort = value;
            OnPropertyChanged();
        }
    }

    public int Baudrate { get; set; } = 1000000;
    public bool CustomBaudrateEnabled { get; set; }
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }
    [JsonIgnore] public string LocalPath { get; set; }
    public string SerialNumber { get; set; }

    private string _firmwareVersion;

    /// <summary>
    /// firmware version read from controller
    /// </summary>
    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set
        {
            _firmwareVersion = value;
            OnPropertyChanged();
        }
    }

    private string _hardwareVersion;

    /// <summary>
    /// hardware version read from controller
    /// </summary>
    public string HardwareVersion
    {
        get => _hardwareVersion;
        set
        {
            _hardwareVersion = value;
            OnPropertyChanged();
        }
    }

    public int DashboardWidth { get; set; }
    public int DashboardHeight { get; set; }
    public double PhysicalWidth { get; set; } = 200;
    public double PhysicalHeight { get; set; } = 200;

    public int HWLVersion { get; set; }
    public LEDController? LedController { get; set; }
    public FanController FanController { get; set; }

    public void RegisterLEDController()
    {
        if (LedController == null)
            return;
        LedController.PropertyChanged += OnControllerPropertyChanged;
    }

    public void RegisterFanController()
    {
        if (FanController == null)
            return;
        FanController.PropertyChanged += OnControllerPropertyChanged;
    }

    private void OnControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(sender));
    }

    [JsonIgnore] public ControllerWorkingStateEnum WorkingStateEnum { get; set; } = ControllerWorkingStateEnum.Normal;

    public void DisableTransfer()
    {
        IsTransferActive = false;
        TransferActiveChanged?.Invoke();
    }

    public void EnterDFU()
    {
        WorkingStateEnum = ControllerWorkingStateEnum.DFU;
        WorkingStateChanged?.Invoke();
    }

    public void ChangeComPort(string port)
    {
        SerialPort = port;
        SerialPortChanged?.Invoke();
    }

    public void Sleep()
    {
        WorkingStateEnum = ControllerWorkingStateEnum.Sleep;
        WorkingStateChanged?.Invoke();
    }

    public void EnableTransfer()
    {
        IsTransferActive = true;
        TransferActiveChanged?.Invoke();
    }

    public void TurnOff()
    {
        WorkingStateEnum = ControllerWorkingStateEnum.Off;
        WorkingStateChanged?.Invoke();
    }

    public void TurnOn()
    {
        WorkingStateEnum = ControllerWorkingStateEnum.Normal;
        WorkingStateChanged?.Invoke();
    }

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.Unknown;
    }

    /// <summary>
    /// Save controller data to json file
    /// </summary>
    public void Save()
    {
        //todo implement profile save with icon 
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = LocalRepository.LocalFolderPath;
            LocalPath = Path.Combine(dbPath, Name + "-" + SerialPort);
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "controller.json"));
    }

    public Bitmap Thumbnail => LoadFromFile(File.Exists(Path.Combine(resourcePath, Name + ".png"))
        ? Path.Combine(resourcePath, Name + ".png")
        : Path.Combine(resourcePath, HardwareType.ToString() + ".png"));

    private Bitmap LoadFromFile(string file)
    {
        if (!File.Exists(file))
            return null;
        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (var memory = new MemoryStream())
        {
            fs.CopyTo(memory);
            memory.Seek(0, SeekOrigin.Begin);
            var bitmap = Bitmap.DecodeToWidth(memory, 400);
            return bitmap;
        }
    }

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }
    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for serial controller
        return null;
    }
}