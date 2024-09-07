using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace AmbinityCore.LightingEngines;

/// <summary>
/// provide brightness map for existing color
/// </summary>
public interface IBrightnessProvider
{
    
    /// <summary>
    /// Get current brightness map
    /// </summary>
    /// <returns></returns>
    void GetBrightness(float[] reusableArray);

    void Activate();
    void Deactivate();


}