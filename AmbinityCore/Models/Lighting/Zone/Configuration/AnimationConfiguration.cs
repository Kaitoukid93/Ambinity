using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class AnimationConfiguration : ObservableObject, ILightingConfiguration
{
    public AnimationConfiguration()
    {
    }

    public AnimationConfiguration(Animation animation)
    {
        Animation = animation;
    }

    public event Action AnimationChanged;
    public ConfigurationType Type => ConfigurationType.Animation;
    public string Name => "Animation";
    public string AnimationFilePath => Path.Combine(Animation.LocalPath, "config.json");
    public string Icon => "LightingConfiguration_Animation";

    public string? GetInfo()
    {
        return "XXX";
    }

    /// <summary>
    /// Config the frame rate of this animation
    /// </summary>
    public int FrameRate { get; set; } = 1;

    /// <summary>
    /// Config the repeat property
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// Set or get the delay frame
    /// </summary>
    public bool DelayFrame { get; set; }

    public void ChangeAnimation(Animation animation)
    {
        Animation = animation;
        OnPropertyChanged(nameof(AnimationFilePath));
        AnimationChanged?.Invoke();
    }

    public Animation Animation { get; set; }
}