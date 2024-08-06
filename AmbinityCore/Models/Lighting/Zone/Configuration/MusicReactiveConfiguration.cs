namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class MusicReactiveConfiguration : ILightingConfiguration
{
    public MusicReactiveConfiguration()
    {
        
    }

    public ConfigurationType Type => ConfigurationType.MusicReactive;
    public string Icon => "LightingConfiguration_MusicReactive";
    public string GetInfo()
    {
        //example
        // 6 Colors, Brightness: 80, Points: 2
        return @"notImplement";
    }
}