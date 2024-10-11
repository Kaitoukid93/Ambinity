using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.QuickAccess;

public class QuickAccessViewModelFactory
{
    private readonly LightingProfileDecoder _decoder;
    private readonly ThumbnailService _thumbnailService;

    public QuickAccessViewModelFactory(LightingProfileDecoder decoder,ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _decoder = decoder;
    }
  
    public ShortcutViewModel GetShortcutViewModel(Shortcut shortcut)
    {
        return new ShortcutViewModel(_decoder,shortcut);
    }
    public QuickAccessDeviceViewModel GetDeviceViewModel(IController controller)
    {
        return new QuickAccessDeviceViewModel(controller,_thumbnailService);
    }
}