using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device;

public class LEDOutput : ObservableObject
{
    public LEDOutput(int maxLed, int index, AmbinityDevice device)
    {
        MaxLED = maxLed;
        Index = index;
        Device = device;
    }
    public int MaxLED { get; set; }
    public int Index { get; set; }
    public AmbinityDevice Device { get; set; }
    public bool IsEnabled { get; set; } = true;
}