namespace AmbinityCore.Models.Lighting.Zone.Configuration;

public enum ConfigurationType
{
    /// <summary>
    /// Require ScreenCapture engine
    /// </summary>
    ScreenCapture,

    /// <summary>
    /// Require ColorPalette engine
    /// </summary>
    ColorPalette,

    /// <summary>
    /// Require StaticColor engine
    /// </summary>
    StaticColor,

    /// <summary>
    /// Require StaticColor engine
    /// </summary>
    BreathingColor,

    /// <summary>
    /// Require MusicReactive engine
    /// </summary>
    MusicReactive,

    /// <summary>
    /// Require Gif image decode and capture engine
    /// </summary>
    Gifxelation,

    /// <summary>
    /// Require Animation engine
    /// </summary>
    Animation,
}