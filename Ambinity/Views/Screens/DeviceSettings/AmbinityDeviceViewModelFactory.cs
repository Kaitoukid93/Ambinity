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
}