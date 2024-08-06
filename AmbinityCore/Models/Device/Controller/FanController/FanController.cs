using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device;

public class FanController : ObservableObject
{
    public string Name { get; set; }
    public string DeviceDescription { get; set; }
    public List<FanOutput> Outputs { get; set; }
    public SerialFanControllerHardwareSettings HardwareSettings { get; set; }
}