using AmbinityCore.Repositories;
using Avalonia.Media;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class StaticColorConfiguration : ILightingConfiguration
{
    public StaticColorConfiguration(int brightness,StaticColor color, bool isBreathing)
    {
        Brightness = brightness;
        IsBreathing = isBreathing;
        Color = color;
    }
    
    public ConfigurationType Type => ConfigurationType.StaticColor;
    /// <summary>
    /// Brightness value in range 0-100
    /// </summary>
    public int Brightness { get; set; }
    /// <summary>
    /// Breathing or static
    /// </summary>
    public bool IsBreathing { get; set; }
    /// <summary>
    /// Color value
    /// </summary>
    public StaticColor Color { get; set; }
    public string Icon => "LightingConfiguration_StaticColor";
    public string GetInfo()
    {
        //example
        // Screen: 1, Brightness: 80, Smooth: 2, Area: 1,2,3,4, 

        return "Color: " + Color.ToString() + ", " + "Brightness: " + Brightness + ", " + "Breathing: " + IsBreathing; 
    }
}