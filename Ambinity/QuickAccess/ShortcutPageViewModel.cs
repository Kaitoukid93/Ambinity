using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class ShortcutPageViewModel : ViewModelBase
{
    private readonly DevicesPageViewModel _devicesPageViewModel;
    private readonly QuickAccessNavigationStore _navigationStore;
    private readonly QuickAccessViewModelFactory _factory;
    private readonly ShortcutRepository _shortcutRepository;

    public ShortcutPageViewModel(DevicesPageViewModel devicesPageViewModel, QuickAccessNavigationStore navigationStore,
        QuickAccessViewModelFactory factory,
        ShortcutRepository shortcutRepository)
    {
        _factory = factory;
        _shortcutRepository = shortcutRepository;
        _shortcutRepository.ItemAdded += OnProfileAdded;
        _navigationStore = navigationStore;
        _devicesPageViewModel = devicesPageViewModel;
        GoToDevicesPageCommand = new RelayCommand(GoToDevicesPage);
        Shortcuts = new ObservableCollection<ShortcutViewModel>();
    }

    public void Init()
    {
        Shortcuts.Clear();
        foreach (var item in _shortcutRepository.Items)
        {
            OnProfileAdded(item);
        }
    }

    private void OnProfileAdded(ICollectableItem item)
    {
        var profile = item as Shortcut;
        var profileVm = _factory.GetShortcutViewModel(profile);
        profileVm.Init();
        Shortcuts.Add(profileVm);
    }

    private void GoToDevicesPage()
    {
        _navigationStore.CurrentViewModel = _devicesPageViewModel;
    }

    public ICommand GoToDevicesPageCommand { get; }
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; set; }
}