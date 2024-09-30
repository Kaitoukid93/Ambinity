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
    public AmbinityDeviceLayoutAssetViewModel(ICollectableItem item,ThumbnailService thumbnailService) : base(item)
    {
       
        _layout = item as AmbinityCore.Models.Device.AmbinityDeviceLayout;
        Name = _layout.Name;
        _thumbnailService = thumbnailService;
    }
    
    private ThumbnailService _thumbnailService;
    private AmbinityCore.Models.Device.AmbinityDeviceLayout _layout;
    public bool IsLocalExisted { get; set; }
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.LoadThumbnail(_layout.Thumbnail);
        return thumb;
    }
    private string _description;

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