using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;

namespace AmbinityCore.LightingEngines;

public class GifxelationEngine : IColorEngine
{
    public GifxelationEngine()
    {
        
    }
    public LightingZone Zone => _zone;
    private LightingZone _zone;
    public void Render()
    {
        
    }
    public void Init(LightingZone zone)
    {
        _zone = zone;
    }
    public bool IsDisposed { get; private set; }
    public void Dispose()
    {
        IsDisposed = true;
        GC.Collect();
    }
    public bool IsAvailable { get; private set; }
    public CapturingType CaptureType { get; set; }
}