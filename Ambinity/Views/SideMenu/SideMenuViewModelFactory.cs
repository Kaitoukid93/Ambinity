using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.SideMenu;

public class SideMenuViewModelFactory
{
    public SideMenuViewModelFactory(LightingProfileRepository profileRepository,
        LightingProfileCategoryRepository categoryRepository, IDialogService dialogService,
        LightingProfileDecoder decoder,
        ThumbnailService thumbnailService, IWindowService windowService)
    {
        _thumbnailService = thumbnailService;
        _profileRepository = profileRepository;
        _categoryRepository = categoryRepository;
        _dialogService = dialogService;
        _decoder = decoder;
        _windowService = windowService;
    }

    private IWindowService _windowService;
    private readonly ThumbnailService _thumbnailService;
    private readonly LightingProfileRepository _profileRepository;
    private readonly LightingProfileCategoryRepository _categoryRepository;
    private readonly IDialogService _dialogService;
    private readonly LightingProfileDecoder _decoder;

    public SideMenuProfileCategoryViewModel GetCategoryViewModel(LightingProfileCategory category)
    {
        return new SideMenuProfileCategoryViewModel(category, _dialogService, _decoder, _profileRepository,
            _windowService, _thumbnailService, this);
    }

    public SideMenuProfileViewModel GetProfileViewModel(LightingProfile profile,
        SideMenuProfileCategoryViewModel category)
    {
        return new SideMenuProfileViewModel(profile, category, _decoder, _thumbnailService, _dialogService, this,
            _windowService);
    }

    public ProfilePropertiesEditorViewModel GetProfilePropertiesViewModel(LightingProfile profile)
    {
        return new ProfilePropertiesEditorViewModel(_thumbnailService, profile, _windowService,_dialogService);
    }
}