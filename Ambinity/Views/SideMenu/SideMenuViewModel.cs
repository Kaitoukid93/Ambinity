using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;

namespace Ambinity.Views.SideMenu;

public class SideMenuViewModel : ViewModelBase
{
    public event Action<SideMenuProfileViewModel> SelectedProfileChanged;
    public event Action<SideMenuScreenViewModel> SelectedScreenChanged;
    public SideMenuViewModel(LightingProfileRepository profileRepository,
        LightingProfileCategoryRepository categoryRepository, IDialogService dialogService)
    {
        _profileRepository = profileRepository;
        _categoryRepository = categoryRepository;
        _dialogService = dialogService;
    }
    
    private LightingProfileRepository _profileRepository;
    private LightingProfileCategoryRepository _categoryRepository;
    private ObservableCollection<SideMenuScreenViewModel> _screenMenuItems;
    private IDialogService _dialogService;
    public ObservableCollection<SideMenuScreenViewModel> ScreenMenuItems
    {
        get { return _screenMenuItems; }
        set
        {
            _screenMenuItems = value;
            RaisePropertyChanged(nameof(ScreenMenuItems));
        }
    }

    private ObservableCollection<SideMenuProfileCategoryViewModel> _profileCategorymenuItems;

    public ObservableCollection<SideMenuProfileCategoryViewModel> ProfileCategorymenuItems
    {
        get { return _profileCategorymenuItems; }
        set
        {
            _profileCategorymenuItems = value;
            RaisePropertyChanged(nameof(ProfileCategorymenuItems));
        }
    }

    private SideMenuScreenViewModel _selectedScreen;

    public SideMenuScreenViewModel SelectedScreen
    {
        get => _selectedScreen;
        set
        {
            _selectedScreen = value;
            RaisePropertyChanged(nameof(SelectedScreen));
            if (value != null)
            {
                ScreenSelectionChanged(SelectedScreen);
            }
        }
    }

    public void Init()
    {
        //get main menu
        ScreenMenuItems = new ObservableCollection<SideMenuScreenViewModel>();
        ProfileCategorymenuItems = new ObservableCollection<SideMenuProfileCategoryViewModel>();
        var homeMenu = new SideMenuScreenViewModel("Dashboard", "Dashboard_Fill");
        var devicemapingMenu = new SideMenuScreenViewModel("Devices", "DeviceMap");
        var settingsMenu = new SideMenuScreenViewModel("Settings", "General_Outline_Settings");
        ScreenMenuItems.Add(homeMenu);
        ScreenMenuItems.Add(devicemapingMenu);
        ScreenMenuItems.Add(settingsMenu);
        foreach (LightingProfileCategory category in _categoryRepository.Items)
        {
            category.FindChild(_profileRepository.Items.ToList());
            var vm = new SideMenuProfileCategoryViewModel(category, _dialogService);
            vm.SelectionChanged += CatergorySelectionChanged;
            ProfileCategorymenuItems.Add(vm);
        }
        //select dashboard
        SelectedScreen = homeMenu;
        //get profile menu, in the future get default from repository
    }

    private void CatergorySelectionChanged(SideMenuProfileViewModel item)
    {
        //unselect all other categories except the one that nonified
        foreach (var vm in ProfileCategorymenuItems.Where(i=>i!=item.Catergory).ToList())
        {
            vm.SelectedProfile = null;
        }

        SelectedScreen = null;
        SelectedProfileChanged?.Invoke(item);
    }

    private void ScreenSelectionChanged(SideMenuScreenViewModel screen)
    {
        //unselect all other categories
        foreach (var vm in ProfileCategorymenuItems)
        {
            vm.SelectedProfile = null;
        }
        SelectedScreenChanged?.Invoke(screen);
    }
}