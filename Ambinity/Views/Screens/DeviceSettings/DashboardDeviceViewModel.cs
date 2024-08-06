using System;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DashboardDeviceViewModel : ViewModelBase
{
    public event Action<DashboardDeviceViewModel> DeviceClicked;

    public DashboardDeviceViewModel(IController controller)
    {
        Controller = controller;
        CommandSetup();
    }

    public IController Controller { get; set; }
    private void CommandSetup()
    {
        DeviceClickedCommand = new RelayCommand(SelectDevice);
    }

    public void SelectDevice()
    {
        DeviceClicked?.Invoke(this);
    }

    public ICommand DeviceClickedCommand { get; set; }
}