using AmbinityCore.CapturingService;

namespace AmbinityCore.LightingEngines;

public class MusicReactiveEngine : IColorEngine
{
    public MusicReactiveEngine()
    {
        CaptureType = CapturingType.AudioCapture;
    }
    public void Render()
    {
        
    }

    public CapturingType CaptureType { get; set; }
}