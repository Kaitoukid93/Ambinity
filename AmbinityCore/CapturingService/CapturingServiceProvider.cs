using AmbinityCore.LightingEngines;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.CapturingService;

public class CapturingServiceProvider(
    AudioCapturingService audioCapturingService,
    ScreenCapturingService screenCapturingService,
    HWMonitorCapturingService hwMonitorCapturingService)
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
                break;
            case CapturingType.AudioCapture:
                return audioCapturingService;
                break;
            case CapturingType.HWCapture:
                return hwMonitorCapturingService;
                break;
        }

        return null;
    }
    public async Task Dispose()
    {
        var tasks = new List<Task>
        {
            Task.Run(() => audioCapturingService.Dispose()),
            Task.Run(() => screenCapturingService.Dispose()),
            Task.Run(() => hwMonitorCapturingService.Dispose())
        };
        await Task.WhenAll(tasks);
    }
    public async Task Init()
    {
        var tasks = new List<Task>
        {
            Task.Run(() => audioCapturingService.Init()),
            Task.Run(() => screenCapturingService.Init()),
            Task.Run(() => hwMonitorCapturingService.Init())
        };
        await Task.WhenAll(tasks);
    }
}
