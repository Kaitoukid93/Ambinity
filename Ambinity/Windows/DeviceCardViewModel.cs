using AmbinityCore.Models.Device;
using Ambinity.ViewModels;
using AmbinityCore.Enums;
using LibreHardwareMonitor.Hardware;
using AmbinityCore.Models.Device.Controller;
using Avalonia.Media.Imaging;

namespace Ambinity.Windows
{
    public class DeviceCardViewModel : ViewModelBase
    {
        public string Name { get; }
        public string Description { get; }
        public Bitmap Thumbnail { get; }
        public HardwareTypeEnum HardwareType { get; }

        public DeviceCardViewModel(IController controller)
        {
            HardwareType = controller.HardwareType;
            Name = controller.Name;
            Description = controller.Description;
            Thumbnail = controller.Thumbnail;
        }
    }
}
