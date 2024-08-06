namespace AmbinityCore.LightingEngines;

public enum RenderingStateEnum
{
    /// <summary>
    /// Indicate the zone is rendering
    /// </summary>
    Render,

    /// <summary>
    /// Indicate the zone paused rendering
    /// </summary>
    Pause,

    /// <summary>
    /// Indicate the zone is disposed
    /// </summary>
    Cancel
}