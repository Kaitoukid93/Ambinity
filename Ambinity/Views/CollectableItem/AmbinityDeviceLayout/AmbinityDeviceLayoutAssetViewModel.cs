using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.CollectableItem.AmbinityDeviceLayout;

public class AmbinityDeviceLayoutAssetViewModel : AssetItemViewModelBase
{
    public AmbinityDeviceLayoutAssetViewModel(ICollectableItem item,Draw2DCanvasViewModel canvasViewModel)
    {
        _canvasViewModel = canvasViewModel;
        _layout = item as AmbinityCore.Models.Device.AmbinityDeviceLayout;
        Name = _layout.Name;
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
        ApplyLayoutCommand = new RelayCommand(ApplyLayout);
    }

    private void ApplyLayout()
    {
       //get all selected device and apply this layout
       var figs = _canvasViewModel.Canvas.Selection.All;
       var selectedDevices = new List<AmbinityDevice>();
       foreach (var fig in figs)
       {
           var deviceContainerFigure = fig as DeviceContainerFigure;
           if (deviceContainerFigure != null)
           {
               selectedDevices.Add(deviceContainerFigure.ChildItem as AmbinityDevice);
           }
       }

       foreach (var device in selectedDevices)
       {
           device.LoadLayout(this._layout);
       }
    }


    private ThumbnailService _thumbnailService;
    private AmbinityCore.Models.Device.AmbinityDeviceLayout _layout;
    public ICommand ApplyLayoutCommand { get; set; }
    public bool IsLocalExisted { get; set; }
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.LoadThumbnail(_layout.Thumbnail);
        return thumb;
    }
    private string _description;
    private readonly Draw2DCanvasViewModel _canvasViewModel;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }
}