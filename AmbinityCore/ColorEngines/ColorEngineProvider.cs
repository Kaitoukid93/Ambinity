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
    private readonly ScreenCapturingService _screenCapturingService;

    public IColorEngine GetEngine(LightingZone zone)
    {
        switch (zone.LightingConfiguration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return Ioc.Default.GetRequiredService<ScreenCaptureEngine>();
                break;
            case ConfigurationType.SelfGeneratedColor:
                return Ioc.Default.GetRequiredService<SelfGeneratedColorEngine>();
                break;
            case ConfigurationType.Gifxelation:
                return Ioc.Default.GetRequiredService<GifxelationEngine>();
                break;
            case ConfigurationType.Animation:
                return Ioc.Default.GetRequiredService<AnimationDecodeEngine>();
                break;
          
        }

        return null;
    }
    
}