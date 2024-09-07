using AmbinityCore.Models.Collection;
using Avalonia.Media.Imaging;

namespace AmbinityCore.Models.Device.Controller;

public interface IController: ICollectableItem
{
    public event Action TransferActiveChanged;
    string Name { get; set; }
    Bitmap Thumbnail { get; }
    string SerialNumber { get; set; }
    string SerialPort { get; set; }
    int DashboardWidth { get; set; }
    int DashboardHeight { get; set; }
    ControllerWorkingStateEnum WorkingStateEnum { get; set; }
    void EnableTransfer();
    void DisableTransfer();
    LEDController LedController { get; set; }
    FanController FanController { get; set; }
    bool IsTransferActive { get; set; }
    void RegisterFanController();
    void RegisterLEDController();
}