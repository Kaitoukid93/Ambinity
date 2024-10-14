using Ambinity.Stores;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.QuickAccess;

public class QuickAccessViewModelFactory
{
    private readonly LightingProfileDecoder _decoder;
    private readonly ThumbnailService _thumbnailService;
    private readonly QuickAccessNavigationStore _navigationStore;
    private readonly ShortcutEditorViewModel _shortcutEditorViewModel;

    public QuickAccessViewModelFactory(LightingProfileDecoder decoder,ThumbnailService thumbnailService,QuickAccessNavigationStore navigationStore, ShortcutEditorViewModel shortcutEditorViewModel)
    {
        _navigationStore = navigationStore;
        _shortcutEditorViewModel = shortcutEditorViewModel;
        _thumbnailService = thumbnailService;
        _decoder = decoder;
    }
  
    public ShortcutViewModel GetShortcutViewModel(Shortcut shortcut)
    {
        return new ShortcutViewModel(_decoder,shortcut,_navigationStore,_shortcutEditorViewModel);
    }
    public QuickAccessDeviceViewModel GetDeviceViewModel(IController controller)
    {
        return new QuickAccessDeviceViewModel(controller,_thumbnailService);
    }
}