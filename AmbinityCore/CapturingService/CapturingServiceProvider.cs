using AmbinityCore.LightingEngines;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.CapturingService;

public class CapturingServiceProvider
{
    public CapturingServiceProvider()
    {
    }

    /// <summary>
    /// provide capturing service for  <param name="engine"></param>
    /// </summary>
    /// <param name="engine"></param>
    /// <returns></returns>
    public ICapturingService GetCapturingService(IColorEngine engine)
    {
        switch (engine.CaptureType)
        {
            case CapturingType.ScreenCapture:
                return Ioc.Default.GetRequiredService<ScreenCapturingService>();
                break;
        }

        return null;
    }
}