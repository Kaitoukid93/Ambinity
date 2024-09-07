namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public enum ConfigurationType
{
    /// <summary>
    /// Require ScreenCapture engine
    /// </summary>
    ScreenCapture,

    /// <summary>
    /// Require Self Generated color engine
    /// </summary>
    SelfGeneratedColor,

    /// <summary>
    /// Require Gif image decode and capture engine
    /// </summary>
    Gifxelation,
    /// <summary>
    /// Require animation engine
    /// </summary>
    Animation
}