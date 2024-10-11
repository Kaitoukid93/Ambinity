using AmbinityCore.Models.Device.Controller;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device;

public class FanOutput : ObservableObject
{
    public FanOutput(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public FanControlModeEnum ControlMode { get; set; } = FanControlModeEnum.Adaptive;
    public int FixedSpeed { get; set; } = 80;
    [JsonIgnore] public int Speed { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}