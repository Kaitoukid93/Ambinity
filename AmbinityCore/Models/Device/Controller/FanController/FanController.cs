using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device;

public class FanController : ObservableObject
{
    public FanController()
    {
        HardwareSettings = new SerialFanControllerHardwareSettings();
        Outputs = new List<FanOutput>();
    }
    public string Name { get; set; }
    public string DeviceDescription { get; set; }
    public List<FanOutput> Outputs { get; set; }
    [JsonIgnore]
    public SerialFanControllerHardwareSettings HardwareSettings { get; set; }
}