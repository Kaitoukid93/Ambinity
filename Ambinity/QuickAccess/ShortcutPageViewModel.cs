using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class ShortcutPageViewModel : ViewModelBase
{
    private readonly DevicesPageViewModel _devicesPageViewModel;
    private readonly QuickAccessNavigationStore _navigationStore;
    private readonly QuickAccessViewModelFactory _factory;
    private readonly ShortcutRepository _shortcutRepository;
    private readonly LightingProfileRepository _lightingProfileRepository;

    public ShortcutPageViewModel(DevicesPageViewModel devicesPageViewModel, QuickAccessNavigationStore navigationStore,
        QuickAccessViewModelFactory factory, LightingProfileRepository profileRepository,
        ShortcutRepository shortcutRepository, IMainWindowService mainWindowService,LightingProfilePlayerWidgetViewModel widgetViewModel, LightingProfileDecoder decoder)
    {
        WidgetViewModel = widgetViewModel;
        _decoder = decoder;
        _decoder.CurrentPlayingProfileChanged += OnCurrentPlayingProfileChanged;
        _lightingProfileRepository = profileRepository;
        _lightingProfileRepository.ItemRemoved += OnLightingProfileRemoved;
        _factory = factory;
        _shortcutRepository = shortcutRepository;
        _shortcutRepository.ItemAdded += OnShortcutAdded;
        _navigationStore = navigationStore;
        _navigationStore.CurrentViewModelChanged += OnNavigated;
        _devicesPageViewModel = devicesPageViewModel;
        _mainWindowService = mainWindowService;
        GoToDevicesPageCommand = new RelayCommand(GoToDevicesPage);
        OpenMainWindowCommand = new RelayCommand<string>(OpenMainWindow);
        EnterEditModeCommand = new RelayCommand(EnterEditMode);
        ExitEditModeCommand = new RelayCommand(ExitEditMode);
        AddShortcutCommand = new RelayCommand(AddShortcut, CanAdd);
        ExitAppCommand = new RelayCommand(RequestAppExit);
        Shortcuts = new ObservableCollection<ShortcutViewModel>();
    }

    private void OnCurrentPlayingProfileChanged(LightingProfile profile)
    {
        ProfileBrightness = profile.Brightness;
    }

    public LightingProfilePlayerWidgetViewModel WidgetViewModel { get; set; }

    private void RequestAppExit()
    {
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime
            controlledApplicationLifetime)
            Dispatcher.UIThread.Post(() => controlledApplicationLifetime.Shutdown());
    }

    private void AddShortcut()
    {
        //add to collection
        var shortcut = new Shortcut("Shortcut "+Shortcuts.Count);
        //add to repository
        _shortcutRepository.AddItem(shortcut);
        AddShortcutCommand.NotifyCanExecuteChanged();
    }

    private void RemoveShortcut(ShortcutViewModel shortcutViewModel)
    {
        //remove from view collection
        Shortcuts.Remove(shortcutViewModel);
        //remove from repository
        _shortcutRepository.RemoveItem(shortcutViewModel.Shortcut);
        AddShortcutCommand.NotifyCanExecuteChanged();
    }
        

    private bool CanAdd()
    {
        return Shortcuts.Count < 6;
    }

    private void EnterEditMode()
    {
        IsInEditMode = true;
        foreach (var shortcut in Shortcuts)
        {
            shortcut.IsInEditMode = true;
        }
        
    }

    private void ExitEditMode()
    {
        foreach (var shortcut in Shortcuts)
        {
            shortcut.IsInEditMode = false;
        }
        IsInEditMode = false;
    }

    private void OpenMainWindow(string screen)
    {
        switch (screen)
        {
            case "home":
                _mainWindowService.OpenMainWindow(0);
                break;
            case "devices":
                _mainWindowService.OpenMainWindow(1);
                break;
            case "layout":
                _mainWindowService.OpenMainWindow(2);
                break;
            case "settings":
                _mainWindowService.OpenMainWindow(3);
                break;
        }
    }

    private void OnNavigated(ViewModelBase viewmodel)
    {
        if(viewmodel==this)
            this.Init();
    }

    private void OnLightingProfileRemoved(ICollectableItem item)
    {
        //update all shortcut can execute status
        foreach (var shortcut in Shortcuts)
        {
            shortcut.CanExecute = shortcut.Shortcut.LightingProfileID != null &&
                                  _lightingProfileRepository.Contains(shortcut.Shortcut.LightingProfileID);
        }
    }


    public void Init()
    {
        Shortcuts?.Clear();
        foreach (var item in _shortcutRepository.Items)
        {
            OnShortcutAdded(item);
        }
        WidgetViewModel.Init();
        OnPropertyChanged(nameof(Shortcuts));
        
    }

    private void OnShortcutAdded(ICollectableItem item)
    {
        var shortcut = item as Shortcut;
        var profileVm = _factory.GetShortcutViewModel(shortcut);
        profileVm.CanExecute = profileVm.Shortcut.LightingProfileID != null &&
                               _lightingProfileRepository.Contains(profileVm.Shortcut.LightingProfileID);
        profileVm.SelfRemoved += RemoveShortcut;
        if (IsInEditMode)
            profileVm.IsInEditMode = true;
        profileVm.Init();
        Shortcuts.Add(profileVm);
    }

    private void GoToDevicesPage()
    {
        _navigationStore.CurrentViewModel = _devicesPageViewModel;
    }

    public ICommand GoToDevicesPageCommand { get; }

    private ObservableCollection<ShortcutViewModel> _shortcut;
    private readonly IMainWindowService _mainWindowService;

    public ObservableCollection<ShortcutViewModel> Shortcuts
    {
        get=>_shortcut;
        set
        {
            _shortcut = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenMainWindowCommand { get; }
    public ICommand EnterEditModeCommand { get; }
    public ICommand ExitEditModeCommand { get; }
    private bool _isInEditMode = false;

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            _isInEditMode = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand AddShortcutCommand { get; }
    public ICommand ExitAppCommand { get; }
    private int _profileBrightness = 80;
    private readonly LightingProfileDecoder _decoder;

    public int ProfileBrightness
    {
        get => _profileBrightness;
        set
        {
            _profileBrightness = value;
            _decoder.CurrentPlayingProfile.Brightness = value;
            OnPropertyChanged();
        }
    }

    public override void Dispose()
    {
        IsInEditMode = false;
    }
}