using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceSettingsViewModel : ViewModelBase
{
    public DeviceSettingsViewModel(RootNavigationStores navigationStores)
    {
        _rootNavigationStores = navigationStores;
    }

    private RootNavigationStores _rootNavigationStores;
    public void Init(IController controller)
    {
        Controller = controller;
        CommandSetup();
    }

    private IController _controller;
    public IController Controller
    {
        get => _controller;
        set
        {
            _controller = value;
            OnPropertyChanged();
        }
    }

    private void CommandSetup()
    {
        BackToDashboardCommand = new RelayCommand(BackToDashboard);
    }

    private void BackToDashboard()
    {
        var vm = Ioc.Default.GetRequiredService<DeviceSettingsDashboardViewModel>();
        
        _rootNavigationStores.CurrentViewModel = vm;
    }

    public ICommand BackToDashboardCommand { get; set; }
}