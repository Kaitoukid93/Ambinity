namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public interface ILightingConfiguration
{
    ConfigurationType Type { get; }
    
    string Name { get; }

    String Icon { get; }

    //just ez way to get info 
    string? GetInfo();
    //todo save config
}