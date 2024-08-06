using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;

namespace AmbinityCore.LightingEngines;

public class AnimationEngine : IColorEngine
{
    public AnimationEngine()
    {
        CaptureType = CapturingType.None;
    }

    public void Render()
    {
        
    }

    public void Init(LightingZone zone)
    {
        
    }

    public bool IsDisposed { get; private set; }
    public void Dispose()
    {
        IsDisposed = true;
        GC.Collect();
    }
    public CapturingType CaptureType { get; set; }
}