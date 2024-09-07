namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public interface IMotionConfiguration
{
    /// <summary>
    /// Type of the motion
    /// </summary>
    MotionTypeEnum Type { get; }

    /// <summary>
    /// Icon of the motion
    /// </summary>
    string Icon { get;}

    public void UpdateConfig();
    event Action Update;
}