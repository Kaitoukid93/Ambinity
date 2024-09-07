using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class SelfGeneratedColorConfiguration : ILightingConfiguration
{
    public event Action? ColorsUpdated;
    public event Action? ApperanceUpdated;
    public event Action? ColorsBehaviorUpdated;
    public event Action? MotionConfigUpdated;

    public SelfGeneratedColorConfiguration()
    {
    }

    public SelfGeneratedColorConfiguration(List<Color> colors, IMotionConfiguration motionConfig)
    {
        Colors = colors;
        MotionConfig = motionConfig;
    }

    public string Name => "Colors";
    public ConfigurationType Type => ConfigurationType.SelfGeneratedColor;

    /// <summary>
    /// Colors value
    /// </summary>
    public List<Color> Colors { get; set; }

    /// <summary>
    /// Get or set appearance property either stroke or fill
    /// </summary>
    public ColorApperance Apperance { get; set; } = new ColorApperance() { Mode = ColorApperanceEnum.Fill, Value = 0 };

    /// <summary>
    /// Get or set Blend mode, either normal or linear
    /// </summary>
    public PaletteBlend Blend { get; set; } = new PaletteBlend() { Mode = PaletteBlendModeEnum.LinearBlend, Value = 0 };

    /// <summary>
    /// Indicate should color be moving in the zone
    /// </summary>
    public bool IsMoving { get; set; } = true;

    /// <summary>
    /// Indicate should color be reversed in the zone
    /// </summary>
    public bool IsReverse { get; set; }

    /// <summary>
    /// How fast color move ( need to set IsMoving to true)
    /// </summary>
    public float Speed { get; set; } = 10f;

    /// <summary>
    /// How dense color place 
    /// </summary>
    public float ColorResolution { get; set; } = 8f;

    /// <summary>
    /// Config how colors moving
    /// </summary>
    public IMotionConfiguration MotionConfig { get; set; }


    public string Icon => "LightingConfiguration_ColorPalette";

    public string GetInfo()
    {
        //example
        // 6 Colors, Brightness: 80, Points: 2
        return Colors.Count.ToString() + " " + "Colors" + ", " +
               "Motion: " + MotionConfig.Type.ToString();
    }

    public void UpdateColors()
    {
        ColorsUpdated?.Invoke();
    }

    public void UpdateColorsBehavior()
    {
        ColorsBehaviorUpdated?.Invoke();
    }

    public void UpdateColorsApperance()
    {
        ApperanceUpdated?.Invoke();
    }

    public void UpdateMotionConfig()
    {
        MotionConfigUpdated?.Invoke();
    }
}