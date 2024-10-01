using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;

namespace AmbinityCore.LightingEngines;

public interface IColorEngine
{
    void Render();
    void Init(LightingZone zone);
    void Dispose();
    bool IsDisposed { get; }
    LightingZone Zone { get;}
}