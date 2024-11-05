using AmbinityCore.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device;

public class LEDOutput : ObservableObject
{
    public event Action<LEDOutput> OutputDisabled;
    public event Action<LEDOutput> OutputEnabled;
    public event Action<LEDOutput> DevicesUpdated;

    public LEDOutput(int maxLed, int index, AmbinityDevice device)
    {
        MaxLED = maxLed;
        Index = index;
        Devices = [device];
    }

    public int MaxLED { get; set; }
    public int Index { get; set; }
    public List<AmbinityDevice> Devices { get; set; }
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

    public void AddDeviceToOutputChain(AmbinityDevice device)
    {
        Devices.Add(device);
        DevicesUpdated?.Invoke(this);
    }

    public void RemoveDeviceFromOutputChain(AmbinityDevice device)
    {
        if (Devices.Contains(device))
        {
            Devices.Remove(device);
            DevicesUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// ping the output to locate 
    /// </summary>
    public async Task PingOutputChain()
    {
        foreach (var device in Devices)
        {
            await device.Ping();
        }
    }

    public int LEDsCount => GetLEDsCount();

    private int GetLEDsCount()
    {
        int ledCount = 0;
        foreach (var device in Devices)
        {
            ledCount += device.Leds.Count;
            
        }

        return ledCount;
    }
}