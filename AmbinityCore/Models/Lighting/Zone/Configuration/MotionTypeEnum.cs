namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public enum MotionTypeEnum
{
    /// <summary>
    /// Colors stay still
    /// </summary>
    None,
    /// <summary>
    /// Animate colors using animation file
    /// </summary>
    Breathing,
    
    /// <summary>
    /// Animate colors using Audio captured data
    /// </summary>
    MusicReactive
}