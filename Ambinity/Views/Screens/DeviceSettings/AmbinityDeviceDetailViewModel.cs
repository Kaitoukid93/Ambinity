using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class AmbinityDeviceDetailViewModel : ViewModelBase
{
    private readonly ThumbnailService _thumbnailService;

    public AmbinityDeviceDetailViewModel(AmbinityDevice device,ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _device = device;
        _layout = _device.Layout;
        OpenLibraryCommand = new RelayCommand(OpenDeviceLibrary);
    }

    private void OpenDeviceLibrary()
    {
       
    }

    private AmbinityCore.Models.Device.AmbinityDeviceLayout _layout;
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();
    public string Name => _device.Name;
    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.LoadThumbnail(_layout.Thumbnail);
        return thumb;
    }
    private readonly AmbinityDevice _device;

    public string Description => _device.DeviceDescription;
    public string LEDsCount => "LEDs count: "+ _device.Leds.Count.ToString();
    public string FilePath =>  "Path: "+ _device.Layout.FilePath;
    public ICommand OpenLibraryCommand { get; set; }
}