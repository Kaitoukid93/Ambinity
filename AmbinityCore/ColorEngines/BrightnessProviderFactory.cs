using AmbinityCore.CapturingService;
using AmbinityCore.CapturingService.AudioCapturing;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Serilog;

namespace AmbinityCore.LightingEngines;

public class BrightnessProviderFactory
{
    public event Action DefaultDeviceChanged;

    public BrightnessProviderFactory(AudioCapturingService capturingService)
    {
        _capturingService = capturingService;
        _capturingService.DefaultDeviceChanged += OnDefaultDeviceChanged;
    }

    private void OnDefaultDeviceChanged()
    {
        DefaultDeviceChanged?.Invoke();
    }

    private AudioCapturingService _capturingService;

    private MusicReactiveBrightnessProvider GetMusicBrightnessProvider(IMotionConfiguration config)
    {
        var brightnessProvider = new MusicReactiveBrightnessProvider(config, _capturingService);
        brightnessProvider.Activate();
        return brightnessProvider;
    }

    public IBrightnessProvider GetBrightnessProvider(IMotionConfiguration configuration)
    {
        switch (configuration.Type)
        {
            case MotionTypeEnum.Breathing:
                return new BreathingBrightnessProvider(configuration);
            //to reduce cpu usage, all music reactive using same device share the same brightness provider
            case MotionTypeEnum.MusicReactive:
                return GetMusicBrightnessProvider(configuration);
            case MotionTypeEnum.None:
                return new StaticBrightnessProvider();
            default: return null;
        }
    }
}