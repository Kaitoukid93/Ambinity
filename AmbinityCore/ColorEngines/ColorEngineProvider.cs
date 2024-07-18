using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.LightingEngines;

public class ColorEngineProvider
{
    /// <summary>
    /// provide background running task with specific color engine
    /// </summary>
    public ColorEngineProvider()
    {
       
    }

    private CapturingServiceProvider _capturingServiceProvider;
    public IColorEngine GetEngine(LightingZone zone)
    {
        switch (zone.LightingConfiguration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return Ioc.Default.GetRequiredService<ScreenCaptureEngine>();
                break;
            case ConfigurationType.ColorPalette:
                return Ioc.Default.GetRequiredService<ColorPaletteEngine>();
                break;
            case ConfigurationType.StaticColor:
            case ConfigurationType.BreathingColor:
                return Ioc.Default.GetRequiredService<StaticColorEngine>();
                break;
            case ConfigurationType.MusicReactive:
                return Ioc.Default.GetRequiredService<MusicReactiveEngine>();
                break;
            case ConfigurationType.Gifxelation:
                return Ioc.Default.GetRequiredService<GifxelationEngine>();
                break;
            case ConfigurationType.Animation:
                return Ioc.Default.GetRequiredService<AnimationEngine>();
                break;
        }

        return null;
    }
    
}