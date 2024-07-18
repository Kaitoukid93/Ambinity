namespace AmbinityCore.CapturingService;

public enum CapturingType
{
    /// <summary>
    /// Require Screen capturing service
    /// </summary>
    ScreenCapture,
    /// <summary>
    /// Require Audio capturing service
    /// </summary>
    AudioCapture,
    /// <summary>
    /// Capturing service is not required
    /// </summary>
    None
}