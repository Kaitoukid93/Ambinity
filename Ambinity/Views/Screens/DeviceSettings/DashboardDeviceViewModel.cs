using System;
using System.Threading.Tasks;
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
        Controller.TransferActiveChanged += OnTransferActiveChanged;
        controller.WorkingStateChanged += OnWorkingStateChanged;
        CommandSetup();
    }

    private void OnWorkingStateChanged()
    {
        OnPropertyChanged(nameof(IsTurnedOn));
    }

    private void OnTransferActiveChanged()
    {
        OnPropertyChanged(nameof(IsTransferActive));
    }

    public bool IsTransferActive => Controller.IsTransferActive;
    public bool IsTurnedOn => Controller.WorkingStateEnum == ControllerWorkingStateEnum.Normal;
    public IController Controller { get; set; }

    private void CommandSetup()
    {
        DeviceClickedCommand = new RelayCommand(SelectDevice);
        TurnOffControllerCommand = new RelayCommand(TurnOff);
        TurnOnControllerCommand = new RelayCommand(TurnOn);
    }

    private void TurnOn()
    {
        Controller.TurnOn();;
    }

    private void TurnOff()
    {
        Controller.TurnOff();
    }

    public void SelectDevice()
    {
        DeviceClicked?.Invoke(this);
    }

    public ICommand DeviceClickedCommand { get; set; }
    public ICommand TurnOffControllerCommand { get; set; }
    public ICommand TurnOnControllerCommand { get; set; }
}