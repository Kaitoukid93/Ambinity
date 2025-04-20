using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class AnimationConfiguration : ObservableObject, ILightingConfiguration
{
    public AnimationConfiguration()
    {
    }

    public AnimationConfiguration(IAnimation  animation)
    {
        if(animation!=null)
        AnimationUID = animation.UID;
    }

    public event Action AnimationChanged;
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
    public int FrameRate { get; set; } = 1;

    /// <summary>
    /// Config the repeat property
    /// </summary>
    public bool Repeat { get; set; }

    /// <summary>
    /// Set or get the delay frame
    /// </summary>
    public bool DelayFrame { get; set; }

    public void ChangeAnimation(IAnimation  animation)
    {
        AnimationUID = animation.UID;
        AnimationChanged?.Invoke();
    }

    public Guid AnimationUID { get; set; }
}
