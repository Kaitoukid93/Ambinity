using AmbinityCore.Models.Device;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.DeviceSettings;

public class AmbinityDeviceViewModelFactory
{
    private readonly ThumbnailService _thumbnailService;

    public AmbinityDeviceViewModelFactory(ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
    }

    public AmbinityDeviceDetailViewModel GetDetailViewModel(AmbinityDevice device)
    {
        return new AmbinityDeviceDetailViewModel(device, _thumbnailService);
    }
    public AmbinityDeviceDaisyChainElementViewModel GetDeviceDaisyChainElementViewModel(AmbinityDevice device)
    {
        return new AmbinityDeviceDaisyChainElementViewModel(device, _thumbnailService);
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