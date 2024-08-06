using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Repositories;
using Avalonia;
using Draw2D.Core.Graphic;
using ScreenCapture.NET;

namespace AmbinityCore.LightingEngines;

public class StaticColorEngine : IColorEngine
{
    public StaticColorEngine(FrameBuffer buffer)
    {
        CaptureType = CapturingType.None;
        _buffer = buffer;
    }
    private LightingZone _zone;
    private FrameBuffer _buffer;
    private StaticColorConfiguration config;
    private Rect zoneRect;
    private Rect zoneAbsoluteRect;
    public void Render()
    {
        var color = (config.Color as SolidColor).Color;
        ColorComputing.SetBlockColor(_buffer, zoneAbsoluteRect, color.R, color.G, color.B);
        ColorComputing.SetBlockColor(_zone.Buffer, zoneRect, color.R, color.G, color.B);
      // _zone.UpdateFrame();
    }
    public void Init(LightingZone zone)
    {
        _zone = zone;
        zoneAbsoluteRect = new Rect(zone.X, zone.Y, zone.Width, zone.Height);
        zoneRect = new Rect(0, 0, zone.Width, zone.Height);
        _zone.UpdateFrameBuffer();
        config = (StaticColorConfiguration)_zone.LightingConfiguration;
    }
    public bool IsDisposed { get; private set; }
    public void Dispose()
    {
        IsDisposed = true;
        GC.Collect();
    }
    public CapturingType CaptureType { get; set; }
}