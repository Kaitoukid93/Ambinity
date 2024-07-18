using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
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

    public void Init()
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
       // var vm = Ioc.Default.GetRequiredService<DeviceControlViewModel>();
      //  vm.Init(device.IledController);
       // _rootNavigationStores.CurrentViewModel = vm;
    }

    private DashboardDeviceViewModel CreateDummyDevice(int port)
    {
        var device = new LEDController();
        device.DeviceName = "Ambino Basic";
        device.DeviceDescription = "USB Lighting Device";
        device.IledControllerHardwareSettings = new SerialLEDControllerHardwareSettings();
        device.IledControllerHardwareSettings.DeviceCommunicationAddress = "COM" + port.ToString();
        device.IsTransferEnabled = false;
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