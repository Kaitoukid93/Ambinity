using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.AppTour;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Screens.AppSettings;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceSettings;
using Ambinity.Views.Screens.Home;
using Ambinity.Views.Screens.ProfileEditor;
using Ambinity.Windows;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.SideMenu;

public class SideMenuViewModel : ViewModelBase, IApptourElement
{
    public event Action<SideMenuProfileViewModel> SelectedProfileChanged;
    public event Action<SideMenuScreenViewModel> SelectedScreenChanged;

    public SideMenuViewModel(LightingProfileRepository profileRepository,
        LightingProfileCategoryRepository categoryRepository, IDialogService dialogService,
        RootNavigationStores rootNavigationStores,
        SideMenuProfilePlayerViewModel profilePlayerViewModel, SideMenuViewModelFactory vmFactory,
        ProfileEditorViewModel profileEditorViewModel, DeviceLayoutEditorViewModel deviceLayoutEditorViewModel,
        AppSettingsViewModel appSettingsViewModel,
        DeviceSettingsDashboardViewModel dashboardViewModel, HomeViewModel homeViewModel, ProfileStoreViewModel storeViewModel)
    {
        _storeViewModel = storeViewModel;
        _homeViewModel = homeViewModel;
        _appSettingsViewModel = appSettingsViewModel;
        _dashboardViewModel = dashboardViewModel;
        _profileEditorViewModel = profileEditorViewModel;
        _deviceLayoutEditorViewModel = deviceLayoutEditorViewModel;
        _vmFactory = vmFactory;
        _categoryRepository = categoryRepository;
        _categoryRepository.ItemAdded += OnNewCategoryAdded;
        _rootNavigationStores = rootNavigationStores;
        _dialogService = dialogService;
        ProfilePlayerViewModel = profilePlayerViewModel;
        ProfilePlayerViewModel.PlayingButtonClicked += OnPlayingButtonClicked;
        _homeViewModel.ShowAllProfileRequested += GotoAmbinityStore;
        CreateNewCategoryCommand = new AsyncRelayCommand(OpenCreateNewProfileDialog);
    }

    private void OnPlayingButtonClicked(LightingProfile profile)
    {
        UnselectCurrentSideMenuItem();
        GoToProfileEditor(profile);
    }

    private void OnNewCategoryAdded(ICollectableItem item)
    {
        var category = item as LightingProfileCategory;
        var vm = _vmFactory.GetCategoryViewModel(category);
        vm.SelectionChanged += CatergorySelectionChanged;
        ProfileCategorymenuItems.Add(vm);
    }

    private async Task OpenCreateNewProfileDialog()
    {
        var vm = new InputDialogContentViewModel();
        vm.DialogClosed += OnCreateNewCategoryDialogClosed;
        await _dialogService.ShowInputDialog(vm, "New category", "Ok", "Cancel");
    }

    private void OnCreateNewCategoryDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as InputDialogContentViewModel;
        var result = (e as ContentDialogClosedEventArgs).Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            //create new profile
            var category = new LightingProfileCategory();
            category.Name = vm.UserInput;
            category.IsDefault = false;
            category.ID = Guid.NewGuid();
            _categoryRepository.AddItem(category);
        }
    }
    
    private LightingProfileCategoryRepository _categoryRepository;
    private ObservableCollection<SideMenuScreenViewModel> _screenMenuItems;
    private IDialogService _dialogService;
    private SideMenuViewModelFactory _vmFactory;

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
    private readonly ProfileEditorViewModel _profileEditorViewModel;
    private readonly DeviceLayoutEditorViewModel _deviceLayoutEditorViewModel;
    private readonly DeviceSettingsDashboardViewModel _dashboardViewModel;
    private readonly AppSettingsViewModel _appSettingsViewModel;
    private readonly HomeViewModel _homeViewModel;
    private readonly ProfileStoreViewModel _storeViewModel;

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

    public void Init(LightingProfile profile = null)
    {
        //configure side menu
        ConfigureSideMenuItem();
        //select dashboard
        if (profile != null)
        {
            GoToProfileEditor(profile);
        }
        else
        {
            SelectedScreen = ScreenMenuItems[0];
        }

        _isInit = true;
    }

    public void Init(int screenIndex)
    {
        //configure side menu
        ConfigureSideMenuItem();
        //select dashboard
        if (screenIndex >= ScreenMenuItems.Count)
            SelectedScreen = ScreenMenuItems[0];
        SelectedScreen = ScreenMenuItems[screenIndex];
        _isInit = true;
    }

    private void ConfigureSideMenuItem()
    {
        ScreenMenuItems = new ObservableCollection<SideMenuScreenViewModel>();
        ProfileCategorymenuItems = new ObservableCollection<SideMenuProfileCategoryViewModel>();
        var homeMenu = new SideMenuScreenViewModel("Home", "home_3__home_house_roof_shelter");
        var deviceSettingsMenu = new SideMenuScreenViewModel("Devices", "Device_settings");
        var deviceLayoutMenu = new SideMenuScreenViewModel("Layout", "map_rounded");
        var ambinityStore = new SideMenuScreenViewModel("Store", "onlineStore");
        var settingsMenu = new SideMenuScreenViewModel("Settings", "settings_future");
        ScreenMenuItems.Add(homeMenu);
        ScreenMenuItems.Add(ambinityStore);
        ScreenMenuItems.Add(deviceSettingsMenu);
        ScreenMenuItems.Add(deviceLayoutMenu);
        ScreenMenuItems.Add(settingsMenu);
        foreach (LightingProfileCategory category in _categoryRepository.Items)
        {
            var vm = _vmFactory.GetCategoryViewModel(category);
            vm.SelectionChanged += CatergorySelectionChanged;
            vm.Delete += OnUserDelete;
            ProfileCategorymenuItems.Add(vm);
        }
    }

    private void OnUserDelete(SideMenuProfileCategoryViewModel category)
    {
        ProfileCategorymenuItems.Remove(category);
        _categoryRepository.RemoveItem(category.Category);
    }

    private void UnselectCurrentSideMenuItem()
    {
        foreach (var vm in ProfileCategorymenuItems)
        {
            vm.SelectedProfile = null;
        }

        SelectedScreen = null;
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
            case "Home":
                GoHome();
                break;
            case "Layout":
                GoToDeviceLayout();
                break;
            case "Devices":
                GoToDeviceSettings();
                break;
            case "Settings":
                GoToAppSettings();
                break;
            case "Store":
                GotoAmbinityStore(null);
                break;
        }
    }

    private async Task GoHome()
    {
        _rootNavigationStores.CurrentViewModel = _homeViewModel;
        _homeViewModel.Init();
    }

    private async Task GoToDeviceLayout()
    {
        // _profileEditorViewModel?.Dispose();
        _rootNavigationStores.CurrentViewModel = _deviceLayoutEditorViewModel;
        _deviceLayoutEditorViewModel.Init();
    }

    // private void GoToDashBoard()
    // {
    //     var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
    //     _rootNavigationStores.CurrentViewModel = vm;
    // }

    //todo take away items init 
    private void GoToProfileEditor(LightingProfile profile)
    {
        // _deviceLayoutEditorViewModel?.Dispose();
        _rootNavigationStores.CurrentViewModel = _profileEditorViewModel;
        _profileEditorViewModel.Init(profile);
    }

    private void GoToDeviceSettings()
    {
        _rootNavigationStores.CurrentViewModel = _dashboardViewModel;
        _dashboardViewModel.Init();
    }

    private void GoToAppSettings()
    {
        _rootNavigationStores.CurrentViewModel = _appSettingsViewModel;
        _appSettingsViewModel.Init();
    }

    private async void GotoAmbinityStore(AssetItemViewModelBase item)
    {
        _rootNavigationStores.CurrentViewModel = _storeViewModel;
        await _storeViewModel.Init(item);
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

    public ICommand CreateNewCategoryCommand { get; set; }

    public override void Dispose()
    {
        // _dashboardViewModel?.Dispose();
        // _deviceLayoutEditorViewModel?.Dispose();
    }

    #region App tour element implementations

    public string Name => "SideMenu";

    public void Deactivate()
    {
        throw new NotImplementedException();
    }

    public void Activate()
    {
        //todo
        //implement app tour script
    }

    public int CurrentStep { get; set; }
    public int StepCount => 5;

    #endregion
}