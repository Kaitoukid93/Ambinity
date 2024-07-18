using AmbinityCore.CapturingService;
using ScreenCapture.NET;

namespace AmbinityCore.LightingEngines;

public class StaticColorEngine : IColorEngine
{
    public StaticColorEngine()
    {
        CaptureType = CapturingType.None;
    }

    public void Render()
    {
        
    }

    public CapturingType CaptureType { get; set; }
}