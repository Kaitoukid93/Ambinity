using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.Views.AmbinityStore;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.SideMenu;

public class SideMenuViewModelFactory
{
    public SideMenuViewModelFactory(LightingProfileRepository profileRepository, IDialogService dialogService,
        LightingProfileDecoder decoder,
        ThumbnailService thumbnailService, IWindowService windowService, AmbinityStoreItemExportViewModel itemExportViewModel)
    {
        _thumbnailService = thumbnailService;
        _profileRepository = profileRepository;
        _dialogService = dialogService;
        _decoder = decoder;
        _windowService = windowService;
        _itemExportViewModel = itemExportViewModel;
    }

    private IWindowService _windowService;
    private readonly ThumbnailService _thumbnailService;
    private readonly LightingProfileRepository _profileRepository;
    private readonly IDialogService _dialogService;
    private readonly LightingProfileDecoder _decoder;
    private readonly AmbinityStoreItemExportViewModel _itemExportViewModel;

    public SideMenuProfileCategoryViewModel GetCategoryViewModel(LightingProfileCategory category)
    {
        return new SideMenuProfileCategoryViewModel(category, _dialogService, _decoder, _profileRepository,
            _windowService, _thumbnailService, this);
    }

    public SideMenuProfileViewModel GetProfileViewModel(LightingProfileItem profile,
        SideMenuProfileCategoryViewModel category)
    {
        return new SideMenuProfileViewModel(profile, category, _decoder, _thumbnailService, _dialogService, this,
            _windowService,_itemExportViewModel);
    }

    public ProfilePropertiesEditorViewModel GetProfilePropertiesViewModel(LightingProfileItem profile)
    {
        return new ProfilePropertiesEditorViewModel(_thumbnailService, profile, _windowService,_dialogService);
    }
}
