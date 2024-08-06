using Avalonia.Media.Imaging;

namespace AmbinityCore.Models.Device.Controller;

public interface IController
{
    string Name { get; set; }
    HardwareTypeEnum HardwareType { get; set; }
    Bitmap Thumbnail { get; }
    string SerialNumber { get; set; }
    string SerialPort { get; set; }
    int DashboardWidth { get; set; }
    int DashboardHeight { get; set; }

}