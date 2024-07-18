using AmbinityCore.CapturingService;

namespace AmbinityCore.LightingEngines;

public class ColorPaletteEngine : IColorEngine
{
    public ColorPaletteEngine()
    {
        CaptureType = CapturingType.None;
    }

    public void Render()
    {
        
    }

    public CapturingType CaptureType { get; set; }
}