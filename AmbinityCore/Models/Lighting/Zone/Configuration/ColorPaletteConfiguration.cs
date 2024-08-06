using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class ColorPaletteConfiguration : ILightingConfiguration
{
    public ColorPaletteConfiguration(int brightness, ColorPalette palette)
    {
        Brightness = brightness;
        Palette = palette;
    }

    public ConfigurationType Type => ConfigurationType.ColorPalette;

    /// <summary>
    /// Brightness value in range 0-100
    /// </summary>
    public int Brightness { get; set; }

    /// <summary>
    /// Colors value
    /// </summary>
    public ColorPalette Palette { get; set; }

    /// <summary>
    /// Animating Path of this config
    /// </summary>
    public List<Point> Points { get; set; }

    /// <summary>
    /// Get or set apperance property
    /// </summary>

    public ColorApperance Apperance { get; set; } = new ColorApperance() { Mode = ColorApperanceEnum.Fill, Value = 0 };

    public PaletteBlend Blend { get; set; } = new PaletteBlend() { Mode = PaletteBlendModeEnum.NoBlend, Value = 0 };
    public string Icon => "LightingConfiguration_ColorPalette";

    public string GetInfo()
    {
        //example
        // 6 Colors, Brightness: 80, Points: 2
        return Palette.Colors.Length.ToString() + " " + "Colors" + ", " + "Brightness: " + Brightness + ", " +
               "Points: " + Points?.Count;
    }
}