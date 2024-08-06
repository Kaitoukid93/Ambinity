namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class AnimationConfiguration : ILightingConfiguration
{
    public AnimationConfiguration()
    {
        
    }

    public ConfigurationType Type => ConfigurationType.Animation;
    public string Icon => "LightingConfiguration_Animation";

    public string GetInfo()
    {
        return "not implement";
    }
}