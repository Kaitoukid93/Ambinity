using AmbinityCore.Models.Collection;
using Avalonia.Media.Imaging;

namespace AmbinityCore.Models.Device.Controller;

public interface IController: ICollectableItem
{
    event Action WorkingStateChanged; 
    event Action TransferActiveChanged;
    string Name { get; set; }
    Bitmap Thumbnail { get; }
    string SerialNumber { get; set; }
    string SerialPort { get; set; }
    string FirmwareVersion { get; set; }
    string HardwareVersion { get; set; }
    int DashboardWidth { get; set; }
    int DashboardHeight { get; set; }
    double PhysicalWidth { get; set; }
    double PhysicalHeight { get; set; }
    ControllerWorkingStateEnum WorkingStateEnum { get; set; }
     HardwareTypeEnum HardwareType { get; set; }
    void EnableTransfer();
    void DisableTransfer();
    LEDController LedController { get; set; }
    FanController FanController { get; set; }
    bool IsTransferActive { get; set; }
    bool AutoConnect { get; set; }
    void RegisterFanController();
    void RegisterLEDController();
    void TurnOff();
    void TurnOn();
}