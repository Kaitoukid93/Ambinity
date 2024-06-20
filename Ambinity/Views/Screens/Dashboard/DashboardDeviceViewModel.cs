using System;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.Dashboard;

public class DashboardDeviceViewModel : ViewModelBase
{
    public event Action<DashboardDeviceViewModel> DeviceClicked;

    public DashboardDeviceViewModel()
    {
        CommandSetup();
    }

    public IDevice Device { get; set; }

    public void Init(IDevice device)
    {
        Device = device;
    }

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