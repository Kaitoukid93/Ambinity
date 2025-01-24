using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.LayoutEditor;
using AmbinityCore;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer;
using AmbinityServer.Download;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.OnlineStore;

public class OnlineItemAssetViewModel : AssetItemViewModelBase
{
    private static bool _canDownload = true;
    private int _thumbnailWidth;

    public OnlineItemAssetViewModel(OnlineItem item, DownloadService downloadService,
        CollectableItemRepository localRepository, int thumbnailWidth = 50) : base()
    {
        _localRepository = localRepository;
        _downloadService = downloadService;
        _thumbnailWidth = thumbnailWidth;
        _item = item;
        Name = _item.Name;
        Description = _item.Description;
        // LastUpdate = item.LastUpdate;
        LastUpdate = _item.LastUpdate;
        FileSize = _item.FileSize;
        DownloadItemCommand = new AsyncRelayCommand(Download);
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
        _ambinityClient = Ioc.Default.GetRequiredService<AmbinityClient>();
        _downloadProgress = new Progress<DownloadProgress>((p) => { CurrentDownloadProgress = p.Progress; });
    }

    ~OnlineItemAssetViewModel()
    {
    }

    public string Tags => String.Join(" - ", OnlineItemData.Tags);

    public async Task<String> GetMarkdownDescription()
    {
        return await _downloadService.GetItemDescription(_item);
    }

    private bool CanDownload()
    {
        return _canDownload;
    }

    private async Task Download()
    {
        //show loading bar
        //download
        if (!_canDownload)
            return;
        _canDownload = false;
        ClearCache();
        IsDownloading = true;
        var downloadPath = Path.Combine(Constants.CacheFolderPath, _item.Name);
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath);
        await _downloadService.DownloadItem(this._item, downloadPath, _downloadProgress);
        //because downloading is too fast, add a bit of delay to notice the user that we're f*king download something
        await Task.Run(() => Task.Delay(1000));
        //call local repository to import data from cache
        _localRepository.ImportItem(Constants.CacheFolderPath);
        //clear cache
        ClearCache();
        IsDownloading = false;
        OnPropertyChanged(nameof(IsLocalExisted));
        _canDownload = true;
    }

    public void ClearCache()
    {
        if (Directory.Exists(Constants.CacheFolderPath))
            Directory.Delete(Constants.CacheFolderPath, true);
    }

    private IProgress<DownloadProgress> _downloadProgress;
    private ThumbnailService _thumbnailService;
    private AmbinityClient _ambinityClient;
    public OnlineItem OnlineItemData => _item;
    private OnlineItem _item;
    public bool IsLocalExisted => _localRepository.Items.Any(i => i.Name == _item.Name);
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    public async Task<List<ScreenshotViewModel>> GetScreenshotAsync()
    {
        var screenshotPath = _item.Path + "/screenshots";
        var screenshots = new List<ScreenshotViewModel>();
        var availableScreenshotPath = await _ambinityClient.SftpServer.GetAllFilesAddressInFolder(screenshotPath);
        if (availableScreenshotPath == null)
            return null;
        foreach (var path in availableScreenshotPath)
        {
            var screenshot = new ScreenshotViewModel(_thumbnailService, path);
            screenshots.Add(screenshot);
        }

        return screenshots;
    }


    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumb = await _thumbnailService.GetThumbnail(_item.ThumbnailPath, _thumbnailWidth);
        return thumb;
    }

    private string _description;
    private readonly DownloadService _downloadService;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    private int _currentDownloadProgress;

    public int CurrentDownloadProgress
    {
        get => _currentDownloadProgress;
        set
        {
            _currentDownloadProgress = value;
            OnPropertyChanged();
        }
    }

    private bool _isDownloading;
    private readonly CollectableItemRepository _localRepository;

    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            _isDownloading = value;
            OnPropertyChanged();
        }
    }

    public string FileSize { get; set; }
    public string LastUpdate { get; set; }
    public ICommand DownloadItemCommand { get; set; }
}