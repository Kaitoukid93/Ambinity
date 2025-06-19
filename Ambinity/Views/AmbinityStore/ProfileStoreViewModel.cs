using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.OnlineStore;
using Ambinity.Views.OnlineStore.Library;
using AmbinityCore.Models.Collection;
using HtmlAgilityPack;

namespace Ambinity.Views.AmbinityStore;

public class ProfileStoreViewModel : ViewModelBase
{
    private LightingProfileLibraryViewModel _libraryViewModel;

    public ProfileStoreViewModel(ProfileStoreNonClientAreaContentViewModel nonClientAreaContentViewModel,
        LightingProfileLibraryViewModel profileLibraryViewModel, AmbinityStoreNavigation storeNavigation,
        AmbinityStoreDetailViewModel detailViewModel,LightingProfileAssetsViewModel assetsViewModel)
    {
        _assetsViewModel = assetsViewModel;
        _assetsViewModel.LoadingChanged += OnLoadingChanged;
        _detailViewModel = detailViewModel;
        _storeNavigation = storeNavigation;
        _libraryViewModel = profileLibraryViewModel;

        NonClientAreaContent = nonClientAreaContentViewModel;
        //init categories
        var colorPaletteProfilesCategory = new ProfileStoreSideMenuItemViewModel()
        {
            Name = "Colors",
            Description = "All profiles using color palette",
            Icon = "paint_bucket__bucket_color_colors_design_paint_painting",
            Filter = ["palette", "Colors", "colors", "color"]
        };
        var musicReactiveProfilesCategory = new ProfileStoreSideMenuItemViewModel()
        {
            Name = "AudioFx",
            Description = "Profiles that react to audio",
            Icon = "audioFX",
            Filter = ["music", "Music", "audio", "Audio"]
        };
        var animationProfilesCategory = new ProfileStoreSideMenuItemViewModel()
        {
            Name = "Animation",
            Description = "Profiles that use lottie animation",
            Icon = "LightingConfiguration_Animation",
            Filter = ["animation", "Animation"]
        };
        var videoProfilesCategory = new ProfileStoreSideMenuItemViewModel()
        {
            Name = "Video",
            Description = "Profiles that use video source",
            Icon = "film_slate__pictures_photo_film_slate",
            Filter = ["video", "Video"]
        };
        var mixProfilesCategory = new ProfileStoreSideMenuItemViewModel()
        {
            Name = "Mixed",
            Description = "Profiles that use video source",
            Icon = "erlenmeyer_flask__science_experiment_lab_flask_chemistry_solution",
            Filter = ["mix", "Mix"]
        };
        SideMenuItems =
        [
            colorPaletteProfilesCategory, musicReactiveProfilesCategory, animationProfilesCategory,
            videoProfilesCategory, mixProfilesCategory
        ];
    }

    private void OnLoadingChanged()
    {
        OnPropertyChanged(nameof(IsLoading));
    }
   public bool IsLoading => _assetsViewModel.IsLoading;
    private async void OnStoreItemSelected(AssetItemViewModelBase item)
    {
        if (item is OnlineItemAssetViewModel)
        {
            var onlineItem = item as OnlineItemAssetViewModel;
            _storeNavigation.CurrentViewModel = _detailViewModel;
            await _detailViewModel.Init(onlineItem);
        }
    }


    public ProfileStoreNonClientAreaContentViewModel NonClientAreaContent { get; set; }

    public List<ProfileStoreSideMenuItemViewModel> SideMenuItems { get; set; }

    //side menu simply a filter
    private ProfileStoreSideMenuItemViewModel _selectedFilter;
    private LightingProfileAssetsViewModel _assetsViewModel;
    private readonly AmbinityStoreNavigation _storeNavigation;
    private readonly AmbinityStoreDetailViewModel _detailViewModel;

    public ProfileStoreSideMenuItemViewModel SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            _selectedFilter = value;
            if (_selectedFilter != null)
            {
                (_libraryViewModel.AssetsViewModel as LightingProfileAssetsViewModel)
                    .FilterItem(_selectedFilter.Filter);
                _storeNavigation.CurrentViewModel = _libraryViewModel;
            }

            OnPropertyChanged();
        }
    }

    public LibraryViewModelBase LibraryViewModel => _libraryViewModel;

    public async Task Init(AssetItemViewModelBase item = null)
    {
        SelectedFilter = null;
        await _libraryViewModel.Init();
        _libraryViewModel.ItemSelected += OnStoreItemSelected;
        if (item != null)
        {
            OnStoreItemSelected(item);
        }
        else
            SelectedFilter = SideMenuItems.First();
    }

    public override void Dispose()
    {
        _libraryViewModel?.Dispose();
    }
}
