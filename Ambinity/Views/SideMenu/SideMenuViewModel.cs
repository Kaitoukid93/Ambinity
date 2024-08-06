using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Screens.Dashboard;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceSettings;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Windows;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public class SideMenuViewModel : ViewModelBase
{
    public event Action<SideMenuProfileViewModel> SelectedProfileChanged;
    public event Action<SideMenuScreenViewModel> SelectedScreenChanged;

    public SideMenuViewModel(LightingProfileRepository profileRepository,
        LightingProfileCategoryRepository categoryRepository, IDialogService dialogService,
        RootNavigationStores rootNavigationStores,
        SideMenuProfilePlayerViewModel profilePlayerViewModel,
        LightingProfileDecoder decoder)
    {
        _profileRepository = profileRepository;
        _categoryRepository = categoryRepository;
        _rootNavigationStores = rootNavigationStores;
        _dialogService = dialogService;
        _decoder = decoder;
        ProfilePlayerViewModel = profilePlayerViewModel;
        ProfilePlayerViewModel.PlayingButtonClicked += GoToProfileEditor;
    }


    private LightingProfileDecoder _decoder;
    private LightingProfileRepository _profileRepository;
    private LightingProfileCategoryRepository _categoryRepository;
    private ObservableCollection<SideMenuScreenViewModel> _screenMenuItems;
    private IDialogService _dialogService;

    private bool _isInit;
    public SideMenuProfilePlayerViewModel ProfilePlayerViewModel { get; set; }

    public ObservableCollection<SideMenuScreenViewModel> ScreenMenuItems
    {
        get { return _screenMenuItems; }
        set
        {
            _screenMenuItems = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<SideMenuProfileCategoryViewModel> _profileCategorymenuItems;

    public ObservableCollection<SideMenuProfileCategoryViewModel> ProfileCategorymenuItems
    {
        get { return _profileCategorymenuItems; }
        set
        {
            _profileCategorymenuItems = value;
            OnPropertyChanged();
        }
    }

    private readonly RootNavigationStores _rootNavigationStores;
    private SideMenuScreenViewModel _selectedScreen;
    private SideMenuProfileViewModel _selectedProfile;

    public SideMenuProfileViewModel SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnPropertyChanged();
        }
    }

    public SideMenuScreenViewModel SelectedScreen
    {
        get => _selectedScreen;
        set
        {
            _selectedScreen = value;
            OnPropertyChanged();
            if (value != null)
            {
                ScreenSelectionChanged(SelectedScreen);
            }
        }
    }

    public void Init()
    {
        //configure side menu
        ConfigureSideMenuItem();
        //select dashboard
        SelectedScreen = ScreenMenuItems[0];
        _isInit = true;
    }

    private void ConfigureSideMenuItem()
    {
        ScreenMenuItems = new ObservableCollection<SideMenuScreenViewModel>();
        ProfileCategorymenuItems = new ObservableCollection<SideMenuProfileCategoryViewModel>();
        var deviceSettingsMenu = new SideMenuScreenViewModel("Devices", "Device_settings");
        var devicemapingMenu = new SideMenuScreenViewModel("Layout", "map_rounded");
        var settingsMenu = new SideMenuScreenViewModel("Settings", "General_Outline_Settings");
        ScreenMenuItems.Add(deviceSettingsMenu);
        ScreenMenuItems.Add(devicemapingMenu);
        ScreenMenuItems.Add(settingsMenu);
        foreach (LightingProfileCategory category in _categoryRepository.Items)
        {
            category.FindChild(_profileRepository.Items.ToList());
            var vm = new SideMenuProfileCategoryViewModel(category, _dialogService, _decoder);
            vm.SelectionChanged += CatergorySelectionChanged;
            ProfileCategorymenuItems.Add(vm);
        }
    }

    private void ScreenSelectionChanged(SideMenuScreenViewModel screen)
    {
        //unselect all other categories
        foreach (var vm in ProfileCategorymenuItems)
        {
            vm.SelectedProfile = null;
        }

        SelectedProfile = null;
        switch (screen.Content)
        {
            case "Dashboard":
                GoToDashBoard();
                break;
            case "Layout":
                GoToDeviceLayout();
                break;
            case "Devices":
                GoToDeviceSettings();
                break;
            case "Settings":
                //gotosettings
                break;
        }
    }

    private async Task GoToDeviceLayout()
    {
        var vm = Ioc.Default.GetRequiredService<DeviceLayoutEditorViewModel>();
        vm.Init();
        _rootNavigationStores.CurrentViewModel = vm;
    }

    private void GoToDashBoard()
    {
        var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
        _rootNavigationStores.CurrentViewModel = vm;
    }

    //todo take away items init 
    private void GoToProfileEditor(LightingProfile profile)
    {
        var vm = Ioc.Default.GetRequiredService<ProfileEditorViewModel>();
        vm.Init(profile);
        _rootNavigationStores.CurrentViewModel = vm;
    }

    private void GoToDeviceSettings()
    {
        var vm = Ioc.Default.GetRequiredService<DeviceSettingsDashboardViewModel>();
        vm.Init();
        _rootNavigationStores.CurrentViewModel = vm;
    }

    private void CatergorySelectionChanged(SideMenuProfileViewModel item)
    {
        //unselect all other categories except the one that nonified
        var unSelectedCategories = ProfileCategorymenuItems.Where(i => i != item.Catergory);
        if (unSelectedCategories != null)
        {
            foreach (var vm in unSelectedCategories)
            {
                vm.SelectedProfile = null;
            }
        }

        //unselect screen
        SelectedProfileChanged?.Invoke(item);
        SelectedScreen = null;
        GoToProfileEditor(item.Profile);
    }

    public override void Dispose()
    {
    }
}