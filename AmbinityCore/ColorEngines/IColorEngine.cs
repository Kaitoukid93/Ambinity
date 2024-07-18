using AmbinityCore.CapturingService;

namespace AmbinityCore.LightingEngines;

public interface IColorEngine
{
    void Render();
    CapturingType CaptureType { get; set; }
}