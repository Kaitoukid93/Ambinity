
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Commands;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.DeviceControl;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    #region Construct

    public DashboardViewModel(NavigationStores navigationStores)
    {
        _navigationStores = navigationStores;
        Devices = new ObservableCollection<DashboardDeviceViewModel>();
        for (int i = 0; i < 5; i++)
        {
            Devices.Add(CreateDummyDevice(i));
        }
    }

    #region Events

    private readonly NavigationStores _navigationStores;
    private void OnDeviceClicked(DashboardDeviceViewModel device)
    {
        var vm = Ioc.Default.GetRequiredService<DeviceControlViewModel>();
        vm.Init(device.Device);
        GotoDeviceControlCommand.Execute(null);
    }
    #endregion

    #endregion

    #region Properties

    public ObservableCollection<DashboardDeviceViewModel> Devices { get; set; }

    #endregion


    #region Methods

    public override void Init()
    {
        //load available devices
        //setup commands
        CommandSetup();
    }
    
    private void CommandSetup()
    {
        var vm = Ioc.Default.GetRequiredService<DeviceControlViewModel>();
        GotoDeviceControlCommand = new NavigateCommand<DeviceControlViewModel>(
            new NavigationService<DeviceControlViewModel>(_navigationStores,vm));
    }
    private DashboardDeviceViewModel CreateDummyDevice(int port)
    {
        var device = new Device();
        device.Name = "Ambino Basic";
        device.Description = "USB Lighting Device";
        device.Address = "COM" + port.ToString();
        device.IsTransferActive = false;
        var deviceVm = new DashboardDeviceViewModel();
        deviceVm.Init(device);
        deviceVm.DeviceClicked += OnDeviceClicked;
        return deviceVm;
    }

    #endregion


    #region Command

    public ICommand GotoDeviceControlCommand { get; set; }

    #endregion
}