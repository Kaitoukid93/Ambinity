namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class GifxelationConfiguration : ILightingConfiguration
{
    public ConfigurationType Type => ConfigurationType.Gifxelation;
    public string Name => "Gifxelation";
    public string Icon => "LightingConfiguration_Gif";
    public string GetInfo()
    {
        //example
        // 6 Colors, Brightness: 80, Points: 2
        return "not implement";
    }
}