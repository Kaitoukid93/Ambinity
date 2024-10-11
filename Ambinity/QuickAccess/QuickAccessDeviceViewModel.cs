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
        Thumbnail = Controller.Thumbnail ??  _thumbnailService.GetThumbnail("null").Result;
    }

    public Bitmap Thumbnail { get; set; }

    public IController Controller { get; set; }
}