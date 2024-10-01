using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DashboardDeviceViewModel : ViewModelBase
{
    public event Action<DashboardDeviceViewModel> DeviceClicked;
private ThumbnailService _thumbnailService;
    public DashboardDeviceViewModel(IController controller, ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        Controller = controller;
        Thumbnail = Controller.Thumbnail ??  _thumbnailService.GetThumbnail("null").Result;
        OnPropertyChanged(nameof(IsOpenRGB));
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
    public Bitmap Thumbnail { get; set; }
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
        Controller.TurnOn();
        ;
    }

    private void TurnOff()
    {
        Controller.TurnOff();
    }

    public void SelectDevice()
    {
        DeviceClicked?.Invoke(this);
    }

    public bool IsOpenRGB => Controller is OpenRGBController;
    public ICommand DeviceClickedCommand { get; set; }
    public ICommand TurnOffControllerCommand { get; set; }
    public ICommand TurnOnControllerCommand { get; set; }
}