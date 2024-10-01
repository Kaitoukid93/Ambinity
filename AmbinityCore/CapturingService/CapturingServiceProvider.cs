using AmbinityCore.LightingEngines;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.CapturingService;

public class CapturingServiceProvider
{
    public CapturingServiceProvider(AudioCapturingService audioCapturingService,ScreenCapturingService screenCapturingService)
    {
        _screenCapturingService = screenCapturingService;
        _audioCapturingService = audioCapturingService;
    }
    private ScreenCapturingService _screenCapturingService;
    private AudioCapturingService _audioCapturingService;
    private HWMonitorCapturingService _hwMonitorCapturingService;
    /// <summary>
    /// provide capturing service for  <param name="engine"></param>
    /// </summary>
    /// <param name="engine"></param>
    /// <returns></returns>
    public ICapturingService GetCapturingService(CapturingType capturingType)
    {
        switch (capturingType)
        {
            case CapturingType.ScreenCapture:
                return _screenCapturingService;
                break;
            case CapturingType.AudioCapture:
                return _audioCapturingService;
                break;
            case CapturingType.HWCapture:
                return _hwMonitorCapturingService;
                break;
        }

        return null;
    }
}