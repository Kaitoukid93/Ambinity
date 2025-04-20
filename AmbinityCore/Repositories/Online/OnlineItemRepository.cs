using System.Collections.ObjectModel;
using adrilight_shared.Models.Store;
using AmbinityCore.Extension;
using AmbinityCore.Models.Collection;
using AmbinityServer;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace AmbinityCore.Repositories;

/// <summary>
/// Base class for display and managing online item
/// </summary>
public abstract class OnlineItemRepository : ObservableObject
{
    public event EventHandler? ServerConnectFailedEvent;
    private int _currentDisplayItemsCount;

    public OnlineItemRepository(AmbinityClient client)
    {
        _client = client;
        Items = new ObservableCollection<OnlineItem>();
        Filters = new List<string>();
    }

    public List<string> Filters { get; set; }
    private AmbinityClient _client;

    /// <summary>
    /// Address of the folder on the server that contains this repository
    /// </summary>
    public string ResourceAddress { get; set; }

    /// <summary>
    /// Available items loaded from server
    /// </summary>
    public ObservableCollection<OnlineItem> Items { get; set; }

    /// <summary>
    /// Name of the repository
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initialize repository
    /// </summary>
    public async Task Init()
    {
        //check server connection
        Items?.Clear();
        _currentDisplayItemsCount = 0;
        var result = await _client.Init();
        if (!result)
        {
            ServerConnectFailedEvent?.Invoke(this, EventArgs.Empty);
            Log.Error("Server not available, please try again");
            return;
        }

        if (ResourceAddress != null)
        {
            LoadAvailableAssets();
        }

        await UpdateCollection("");
    }

    private async Task LoadAvailableAssets()
    {
        AvailableAssetsPaths?.Clear();
        var itemsFolder = await _client.SftpServer.GetAllFilesAddressInFolder(ResourceAddress);
        if (itemsFolder == null)
        {
            Log.Information("Folder is empty: " + ResourceAddress);
            return;
        }

        foreach (var folder in itemsFolder)
        {
            if (!_client.SftpServer.IsFolder(folder))
                continue;
            AvailableAssetsPaths.Add(folder);
        }

        await GetFilters();
        Log.Information("Collection updated: Items count = " + AvailableAssetsPaths.Count);
    }

    private List<string> AvailableAssetsPaths { get; set; } = [];

    private string _currentFilter = "";
    // public async Task UpdateCollection()
    // {
    //     if (_currentDisplayItemsCount == AvailableAssetsPaths.Count-1)
    //         return;
    //     Log.Information("Updating collection: " + ResourceAddress);
    //     foreach (var url in AvailableAssetsPaths.Take(new Range(_currentDisplayItemsCount,
    //                  _currentDisplayItemsCount + 10)))
    //     {
    //         var infoPath = url + "/info.json";
    //         var contentPath = url + "/content/";
    //         var item = _client.SftpServer.GetFiles<OnlineItem>(infoPath).Result;
    //         item.Path = url;
    //         //var item = Convert(info);
    //         item.ThumbnailPath = url + "/thumb.png";
    //         item.LastUpdate = _client.SftpServer.GetFileAttributes(infoPath).LastWriteTime.ToString("MMMM dd, yyyy");
    //         try
    //         {
    //             var contents = await _client.SftpServer.GetAllFilesAddressInFolder(contentPath);
    //             long fileSize = 0;
    //             foreach (var file in contents)
    //             {
    //                 fileSize += _client.SftpServer.GetFileAttributes(file).Size;
    //             }
    //
    //             item.FileSize = fileSize.ToSize(FileExtension.SizeUnits.KB) + " KB";
    //         }
    //         catch (Exception e)
    //         {
    //             Log.Warning("No content found");
    //         }
    //
    //         AddItem(item);
    //         _currentDisplayItemsCount ++;
    //     }
    //
    //     
    // }

    public async Task UpdateCollection(string filter)
    {
        //clear collection each time user search
        if (_currentFilter != filter)
        {
            _currentFilter = filter;
            _currentDisplayItemsCount = 0;
            Items?.Clear();
        }
        var filteredFolder = new List<string>();
        if (_currentFilter != null && _currentFilter != string.Empty)
        {
            Log.Information("Updating collection: " + _currentFilter);
            filteredFolder = AvailableAssetsPaths
                .Where(i => i.ToLower().Contains(_currentFilter))
                .ToList();
        }
        else
        {
            filteredFolder = AvailableAssetsPaths;
        }

        foreach (var url in filteredFolder.Take(new Range(_currentDisplayItemsCount,
                     _currentDisplayItemsCount + 10)))
        {
            var infoPath = url + "/info.json";
            var contentPath = url + "/content/";
            var item = _client.SftpServer.GetFiles<OnlineItem>(infoPath).Result;
            item.Path = url;
            //var item = Convert(info);
            item.ThumbnailPath = url + "/thumb.png";
            item.LastUpdate = _client.SftpServer.GetFileAttributes(infoPath).LastWriteTime.ToString("MMMM dd, yyyy");
            try
            {
                var contents = await _client.SftpServer.GetAllFilesAddressInFolder(contentPath);
                long fileSize = 0;
                foreach (var file in contents)
                {
                    fileSize += _client.SftpServer.GetFileAttributes(file).Size;
                }

                item.FileSize = fileSize.ToSize(FileExtension.SizeUnits.KB) + " KB";
            }
            catch (Exception e)
            {
                Log.Warning("No content found");
            }

            AddItem(item);
            _currentDisplayItemsCount++;
        }
    }

    private async Task GetFilters()
    {
        var filterPath = ResourceAddress + "/filters.json";
        if (!_client.SftpServer.IsExist(filterPath))
            return;
        var filters = await Task.Run(() => _client.SftpServer.GetFiles<List<StoreFilterModel>>(filterPath).Result);
        foreach (var fil in filters)
        {
            Filters.Add(fil.Name);
        }
    }

    /// <summary>
    /// Convert Legacy online Item
    /// </summary>
    private OnlineItem Convert(OnlineItemModel legacyItem)
    {
        var convertedItem = new OnlineItem();
        convertedItem.Name = legacyItem.Name;
        convertedItem.Path = legacyItem.Path;
        convertedItem.Owner = legacyItem.Owner;
        convertedItem.Description = legacyItem.Owner;
        convertedItem.Version = legacyItem.Version;
        convertedItem.Type = GetType(legacyItem.Type);
        return convertedItem;
    }

    private OnlineItemTypeEnum GetType(string type)
    {
        switch (type)
        {
            case "ARGBLEDSlaveDevice":
                return OnlineItemTypeEnum.DeviceLayout;
            case "ColorPalette":
                return OnlineItemTypeEnum.ColorPalette;
            case "Gif":
                return OnlineItemTypeEnum.Gif;
            case "ChasingPattern":
                return OnlineItemTypeEnum.Animation;
            default:
                return OnlineItemTypeEnum.Unknown;
        }
    }

    public void CheckServerConnection()
    {
        _client.Init();
    }

    public void AddItem(OnlineItem item)
    {
        RegisterItem(item);
        Items.Add(item);
    }

    /// <summary>
    /// Register when new item got insert or add
    /// </summary>
    private void RegisterItem(OnlineItem item)
    {
    }
}