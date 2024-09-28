using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Ambinity.Views.Screens.DeviceLayout;
using Ambinity.Views.Screens.DeviceSettings;
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
        ProfileEditorViewModel profileEditorViewModel, DeviceLayoutEditorViewModel deviceLayoutEditorViewModel, DeviceSettingsDashboardViewModel dashboardViewModel)
    {
        _dashboardViewModel = dashboardViewModel;
        _profileEditorViewModel = profileEditorViewModel;
        _deviceLayoutEditorViewModel = deviceLayoutEditorViewModel;
        _vmFactory = vmFactory;
        _profileRepository = profileRepository;
        _categoryRepository = categoryRepository;
        _categoryRepository.ItemAdded += OnNewCategoryAdded;
        _rootNavigationStores = rootNavigationStores;
        _dialogService = dialogService;
        ProfilePlayerViewModel = profilePlayerViewModel;
        ProfilePlayerViewModel.PlayingButtonClicked += OnPlayingButtonClicked;
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

    private LightingProfileRepository _profileRepository;
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
        var deviceLayoutMenu = new SideMenuScreenViewModel("Layout", "map_rounded");
        var settingsMenu = new SideMenuScreenViewModel("Settings", "General_Outline_Settings");
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
            // case "Dashboard":
            //     GoToDashBoard();
            //     break;
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
        _profileEditorViewModel?.Dispose();
        _deviceLayoutEditorViewModel.Init();
        _rootNavigationStores.CurrentViewModel = _deviceLayoutEditorViewModel;
    }

    // private void GoToDashBoard()
    // {
    //     var vm = Ioc.Default.GetRequiredService<DashboardViewModel>();
    //     _rootNavigationStores.CurrentViewModel = vm;
    // }

    //todo take away items init 
    private void GoToProfileEditor(LightingProfile profile)
    {
        _deviceLayoutEditorViewModel?.Dispose();
        _profileEditorViewModel.Init(profile);
        _rootNavigationStores.CurrentViewModel = _profileEditorViewModel;
    }

    private void GoToDeviceSettings()
    {
        _dashboardViewModel.Init();
        _rootNavigationStores.CurrentViewModel = _dashboardViewModel;
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
        _dashboardViewModel?.Dispose();
        _deviceLayoutEditorViewModel?.Dispose();
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