using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.OnlineStore;

public class OnlineItemAssetViewModel : AssetItemViewModelBase
{
    public OnlineItemAssetViewModel(OnlineItem item)
    {
        _item = item;
        Name = _item.Name;
        Description = _item.Description;
        LastUpdate = item.LastUpdate;
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
    }
    

    private ThumbnailService _thumbnailService;
    private OnlineItem _item;
    public ICommand DownloadItem { get; set; }
    public bool IsLocalExisted { get; set; }
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.GetThumbnail(_item.ThumbnailPath);
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
