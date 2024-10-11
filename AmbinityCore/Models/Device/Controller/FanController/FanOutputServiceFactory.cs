using AmbinityCore.CapturingService;

namespace AmbinityCore.Models.Device;

public class FanOutputServiceFactory
{
    private CapturingServiceProvider _capturingServiceProvider;

    public FanOutputServiceFactory(CapturingServiceProvider capturingServiceProvider)
    {
        _capturingServiceProvider = capturingServiceProvider;
    }

    public FanOutputService GetFanOutputService(FanOutput output)
    {
        return new FanOutputService(_capturingServiceProvider, output);
    }
}