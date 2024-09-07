namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public class NoneMotionConfiguration : IMotionConfiguration
{
    public MotionTypeEnum Type => MotionTypeEnum.None;
    public string Icon { get; }
    public event Action Update;

    public void UpdateConfig()
    {
        
    }
}