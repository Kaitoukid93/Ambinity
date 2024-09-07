namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public interface IVisualizerStyle
{ 
    string Name { get; }
    VisualizerStyleEnum Type { get; }
    float Sensitivity { get; set; }
    
}