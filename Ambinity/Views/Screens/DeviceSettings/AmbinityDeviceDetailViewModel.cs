using System.IO;
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

    public AmbinityDeviceDetailViewModel(AmbinityDevice device, ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _device = device;
        _device.DeviceUpdate += OnDeviceUpdated;
        _layout = _device.Layout;
        Name = _device.Name;
        Description = _device.DeviceDescription;
        LEDsCount = "LEDs count: " + _device.Leds.Count.ToString();
        FilePath = "Path: " + _device.Layout.FilePath;
    }

    private void OnDeviceUpdated()
    {
        _layout = _device.Layout;
        Name = _device.Name;
        Description = _device.DeviceDescription;
        LEDsCount = "LEDs count: " + _device.Leds.Count.ToString();
        FilePath = "Path: " + _device.Layout.FilePath;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(LEDsCount));
        OnPropertyChanged(nameof(FilePath));
        OnPropertyChanged(nameof(GetThumbnail));
    }

    public AmbinityDeviceDetailViewModel(int deviceCount, ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        Name = "Multiple items selected";
        OnPropertyChanged(nameof(Name));
        LEDsCount = "Settings will apply to all selected items";
        OnPropertyChanged(nameof(Description));
        IsMultipleItemsSelected = true;
    }

    public AmbinityDeviceDetailViewModel()
    {
    }

    private AmbinityCore.Models.Device.AmbinityDeviceLayout _layout;
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();
    public string Name { get; set; }

    private async Task<Bitmap> GetThumbnailAsync()
    {
        if (!File.Exists(_layout.Thumbnail))
            return await _thumbnailService.LoadThumbnail("null");
        var thumb = await _thumbnailService.LoadThumbnail(_layout.Thumbnail);
        return thumb;
    }

    private readonly AmbinityDevice _device;

    public string Description { get; set; }
    private bool _isMultipleItemsSelected;

    public bool IsMultipleItemsSelected
    {
        get => _isMultipleItemsSelected;
        set
        {
            _isMultipleItemsSelected = value;
            OnPropertyChanged();
        }
    }

    public string LEDsCount { get; set; }
    public string FilePath { get; set; }
}