using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.DeviceControl;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    #region Construct

    public DashboardViewModel(RootNavigationStores rootNavigationStores)
    {
        _rootNavigationStores = rootNavigationStores;
    }

    #region Events

    private readonly RootNavigationStores _rootNavigationStores;

    private void OnDeviceClicked(DashboardDeviceViewModel device)
    {
        GotoDeviceControlCommand.Execute(device);
    }

    #endregion

    #endregion

    #region Properties

    public ObservableCollection<DashboardDeviceViewModel> Devices { get; set; }

    #endregion


    #region Methods

    public override void Init(object parameter = null)
    {
        //load available devices
        //setup commands
        Devices = new ObservableCollection<DashboardDeviceViewModel>();
        for (int i = 0; i < 5; i++)
        {
            Devices.Add(CreateDummyDevice(i));
        }

        CommandSetup();
    }

    private void CommandSetup()
    {
        GotoDeviceControlCommand = new RelayCommand<DashboardDeviceViewModel>(GoToDeviceControl);
    }

    private void GoToDeviceControl(DashboardDeviceViewModel device)
    {
        var vm = Ioc.Default.GetRequiredService<DeviceControlViewModel>();
        vm.Init(device.Device);
        _rootNavigationStores.CurrentViewModel = vm;
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