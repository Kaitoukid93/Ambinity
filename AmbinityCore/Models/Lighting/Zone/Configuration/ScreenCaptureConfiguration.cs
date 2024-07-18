using Avalonia;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class ScreenCaptureConfiguration : ILightingConfiguration
{
    public ScreenCaptureConfiguration( int brightness, int smooth, Rect screenCaptureArea, int displayIndex,bool brightnessCorrection)
    {
        Brightness = brightness;
        Smooth = smooth;
        ScreenCaptureArea = screenCaptureArea;
        DisplayIndex = displayIndex;
        BrightnessCorrection = brightnessCorrection;
    }

    public ScreenCaptureConfiguration()
    {
        
    }
    public ConfigurationType Type => ConfigurationType.ScreenCapture;

    /// <summary>
    /// Brightness value in range 0-100
    /// </summary>
    public int Brightness { get; set; }

    /// <summary>
    /// smooth value in range 0-6
    /// </summary>
    public int Smooth { get; set; }

    /// <summary>
    /// Indicate the area the zone capture from
    /// </summary>
    public Rect ScreenCaptureArea { get; set; }

    /// <summary>
    /// Indicate which display the zone capture from
    /// </summary>
    public int DisplayIndex { get; set; }

    /// <summary>
    /// Switch between Linear and Non-Linear Lighting
    /// </summary>
    public bool BrightnessCorrection { get; set; }
}