using Newtonsoft.Json;
using SkiaSharp.Skottie;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class AnimationConfiguration : ILightingConfiguration
{
    public ConfigurationType Type => ConfigurationType.Animation;
    public string Name => "Animation";
    public string Icon => "LightingConfiguration_Animation";

    public string? GetInfo()
    {
        return "XXX";
    }

    /// <summary>
    /// Config the frame rate of this animation
    /// </summary>
    public int FrameRate { get; set; }

    /// <summary>
    /// Config the repeat property
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// Set or get the delay frame
    /// </summary>
    public bool DelayFrame { get; set; }
    
    [JsonIgnore]
    public Animation Animation { get; set; }
}