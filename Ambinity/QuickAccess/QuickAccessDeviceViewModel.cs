using Ambinity.ViewModels;
using AmbinityCore.Models.Device.Controller;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;

namespace Ambinity.QuickAccess;

public class QuickAccessDeviceViewModel : ViewModelBase
{
    private readonly ThumbnailService _thumbnailService;

    public QuickAccessDeviceViewModel(IController controller,ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        Controller = controller;
        IsActive = Controller.WorkingStateEnum == ControllerWorkingStateEnum.Normal;
        Thumbnail = Controller.Thumbnail ??  _thumbnailService.GetThumbnail("null").Result;
    }

 

    public Bitmap Thumbnail { get; set; }

    public IController Controller { get; set; }
    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (value)
            {
                Controller.TurnOn();
            }
            else
            {
                Controller.TurnOff();
            }
            OnPropertyChanged();
        }

    }
}