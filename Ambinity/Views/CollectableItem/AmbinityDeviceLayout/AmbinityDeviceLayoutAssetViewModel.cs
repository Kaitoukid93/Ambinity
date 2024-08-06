using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.CollectableItem.AmbinityDeviceLayout;

public class AmbinityDeviceLayoutAssetViewModel : AssetItemViewModelBase
{
    public AmbinityDeviceLayoutAssetViewModel(ICollectableItem item)
    {
        _item = item as AmbinityCore.Models.Device.AmbinityDeviceLayout;
        Name = _item.Name;
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
    }
    

    private ThumbnailService _thumbnailService;
    private AmbinityCore.Models.Device.AmbinityDeviceLayout _item;
    public ICommand ApplyLayout { get; set; }
    public bool IsLocalExisted { get; set; }
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.LoadThumbnail(_item.Thumbnail);
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
    public DateTime LastUpdate { get; set; }
}