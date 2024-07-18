using AmbinityCore.CapturingService;

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

    public CapturingType CaptureType { get; set; }
}