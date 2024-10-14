using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class QuickAccessViewModel : ViewModelBase
{
    private readonly QuickAccessNavigationStore _navigationStore;
    private readonly DevicesPageViewModel _devicesPageViewModel;
    private readonly ShortcutPageViewModel _shortcutPageViewModel;

    public QuickAccessViewModel(QuickAccessNavigationStore navigationStore, ShortcutPageViewModel shortcutPageViewModel, DevicesPageViewModel devicesPageViewModel)
    {
        _shortcutPageViewModel = shortcutPageViewModel;
        _devicesPageViewModel = devicesPageViewModel;
        _navigationStore = navigationStore;
        GoToDevicesPageCommand = new RelayCommand(GoToDevicesPage);

    }

    private void GoToDevicesPage()
    {
        _devicesPageViewModel.Init();
        _navigationStore.CurrentViewModel = _devicesPageViewModel;
    }

    public ICommand GoToDevicesPageCommand { get; }

    public void Init()
    {
        _navigationStore.CurrentViewModel = _shortcutPageViewModel;
    }

    public override void Dispose()
    {
        _shortcutPageViewModel?.Dispose();
        _devicesPageViewModel?.Dispose();
    }
}