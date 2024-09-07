namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class BrightnessVisualizerStyle: IVisualizerStyle
{
    public string Name => "Brightness";
    public VisualizerStyleEnum Type => VisualizerStyleEnum.Brightness;
    public float Sensitivity { get; set; }
}