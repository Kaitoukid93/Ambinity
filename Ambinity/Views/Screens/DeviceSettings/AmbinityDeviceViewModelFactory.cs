using Ambinity.Services;
using AmbinityCore.Models.Device;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.DeviceSettings;

public class AmbinityDeviceViewModelFactory
{
    private readonly ThumbnailService _thumbnailService;
    private readonly IWindowService _windowService;

    public AmbinityDeviceViewModelFactory(ThumbnailService thumbnailService, IWindowService windowService)
    {
        _thumbnailService = thumbnailService;
        _windowService = windowService;
    }

    public AmbinityDeviceDetailViewModel GetDetailViewModel(AmbinityDevice device)
    {
        return new AmbinityDeviceDetailViewModel(device, _thumbnailService,_windowService);
    }
    public AmbinityDeviceDaisyChainElementViewModel GetDeviceDaisyChainElementViewModel(AmbinityDevice device,DevicePortViewModel port, int index)
    {
        return new AmbinityDeviceDaisyChainElementViewModel(device, _thumbnailService,port, index);
    }
    public AmbinityDeviceDetailViewModel GetMultipleDetailViewModel(int itemCount)
    {
        return new AmbinityDeviceDetailViewModel(itemCount, _thumbnailService );
    }
    public AmbinityDeviceDetailViewModel GetNullDetailViewModel()
    {
        return new AmbinityDeviceDetailViewModel();
    }
}