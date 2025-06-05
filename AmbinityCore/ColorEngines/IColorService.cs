using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;

namespace AmbinityCore.LightingEngines;

public interface IColorService
{
    void Render();
    void Init(LightingZone zone);
    void Dispose();
    bool IsDisposed { get; }
    bool IsAvailable { get; }
    LightingZone Zone { get;}
}
