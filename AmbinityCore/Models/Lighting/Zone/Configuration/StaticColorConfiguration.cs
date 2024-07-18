namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class StaticColorConfiguration : ILightingConfiguration
{
    public StaticColorConfiguration()
    {
        
    }
    public ConfigurationType Type => ConfigurationType.StaticColor;
}