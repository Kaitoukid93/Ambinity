using AmbinityCore.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device;

public class LEDOutput : ObservableObject
{
    public event Action<LEDOutput>  OutputDisabled;
    public event Action<LEDOutput> OutputEnabled;

    public LEDOutput(int maxLed, int index, AmbinityDevice device)
    {
        MaxLED = maxLed;
        Index = index;
        Device = device;
    }

    public int MaxLED { get; set; }
    public int Index { get; set; }
    public AmbinityDevice Device { get; set; }
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (value)
                OutputEnabled?.Invoke(this);
            else
            {
                OutputDisabled?.Invoke(this);
            }
        }
    }

    public LEDOutputPosition OutputPosition { get; set; }

    public int Brightness { get; set; } = 150;

    public void SetBrightness(int value)
    {
        if (value < 0 || value > 255)
            return;
        Brightness = value;
    }
   
    public RGBLEDOrderEnum RGBOrder { get; set; }
   
}