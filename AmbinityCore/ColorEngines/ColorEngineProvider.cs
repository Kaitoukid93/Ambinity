using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.LightingEngines;

public class ColorServiceProvider
{
    /// <summary>
    /// provide background running task with specific color engine
    /// </summary>
    public ColorServiceProvider()
    {

    }

    private CapturingServiceProvider _capturingServiceProvider;
    private readonly ScreenCapturingService _screenCapturingService;

    public IColorService GetService(LightingZone zone)
    {
        switch (zone.LightingConfiguration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return Ioc.Default.GetRequiredService<ScreenCaptureEngine>();
            case ConfigurationType.SelfGeneratedColor:
                return Ioc.Default.GetRequiredService<SelfGeneratedColorEngine>();
            case ConfigurationType.Gifxelation:
                return Ioc.Default.GetRequiredService<GifxelationEngine>();
            case ConfigurationType.Animation:
                return Ioc.Default.GetRequiredService<AnimationDecodeEngine>();

        }

        return null;
    }

}
