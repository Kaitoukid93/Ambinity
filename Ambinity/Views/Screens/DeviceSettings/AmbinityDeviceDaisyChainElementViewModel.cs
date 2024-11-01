using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.DeviceSettings;

public class AmbinityDeviceDaisyChainElementViewModel : DaisyChainItemViewModelBase
{
    private readonly ThumbnailService _thumbnailService;
    public AmbinityDevice Device => _device;
    /// <summary>
    /// occur when user press button and flyout open
    /// </summary>
    public event Action<AmbinityDeviceDaisyChainElementViewModel> Selected;
    public event Action<AmbinityDeviceDaisyChainElementViewModel> Detach;
    public event Action<AmbinityDeviceDaisyChainElementViewModel> ChangeDevice;
    public AmbinityDeviceDaisyChainElementViewModel(AmbinityDevice device, ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
        _device = device;
        _device.DeviceUpdate += OnDeviceUpdated;
        _layout = _device.Layout;
        Name = _device.Name;
        Description = _device.DeviceDescription;
        LEDsCount = "LEDs count: " + _device.Leds.Count.ToString();
        FilePath = "Path: " + _device.Layout.FilePath;
        SelectDeviceCommand = new RelayCommand(SelectDevice);
        DetachThisDeviceCommand = new RelayCommand(DetachThisDevice);
        OpenLibraryCommand = new RelayCommand(OpenLibrary);
    }

    private void OpenLibrary()
    {
        ChangeDevice?.Invoke(this);
    }

    private void DetachThisDevice()
    {
        Detach?.Invoke(this);
    }

    private void SelectDevice()
    {
        Selected?.Invoke(this);
    }

    public RelayCommand SelectDeviceCommand { get; set; }


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


    public AmbinityDeviceDaisyChainElementViewModel()
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
    public string LEDsCount { get; set; }
    public string FilePath { get; set; }
    public ICommand OpenLibraryCommand { get; }
    public ICommand DetachThisDeviceCommand { get; }
}