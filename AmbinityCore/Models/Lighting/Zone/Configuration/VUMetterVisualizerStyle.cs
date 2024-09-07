namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class VUMetterVisualizerStyle : IVisualizerStyle
{
    public string Name => "VUMetter";
    public VisualizerStyleEnum Type => VisualizerStyleEnum.VUMetter;
    public float Sensitivity { get; set; }
    public VUVisualizerMode VisualizerMode { get; set; }
}