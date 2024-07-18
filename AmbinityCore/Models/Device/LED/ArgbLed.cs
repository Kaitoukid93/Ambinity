using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Device.LED;

public class ArgbLed : ObservableObject
{
    public byte Red { get; private set; }
    public byte Green { get; private set; }
    public byte Blue { get; private set; }
    /// <summary>
    /// Set led's color with red, green and blue values, raise events to true to update view
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    /// <param name="raiseEvents"></param>
    public void SetColor(byte red, byte green, byte blue, bool raiseEvents)
    {
        Red = red;
        Green = green;
        Blue = blue;
        if (raiseEvents)
        {
            OnPropertyChanged(nameof(OnDemandColor));
        }
    }
    public Color OnDemandColor => Color.FromRgb(Red, Green, Blue);
}