using AmbinityCore.LightingEngines;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AmbinityCore.CapturingService;

public class CapturingServiceProvider(
    [FromKeyedServices("AudioCapturing")] ICapturingService audioCapturingService,
    [FromKeyedServices("ScreenCapturing")] ICapturingService screenCapturingService,
    [FromKeyedServices("HWCapturing")] ICapturingService hwMonitorCapturingService)
{
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
                return screenCapturingService;
               
            case CapturingType.AudioCapture:
                return audioCapturingService;
                
            case CapturingType.HWCapture:
                return hwMonitorCapturingService;
                
        }

        return null;
    }
}