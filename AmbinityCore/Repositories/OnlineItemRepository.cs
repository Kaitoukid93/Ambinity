using adrilight_shared.Models.Store;
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

    public OnlineItemRepository(AmbinityClient client)
    {
        _client = client;
        Items = new List<OnlineItem>();
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
    public List<OnlineItem> Items { get; set; }

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
        var result = await _client.Init();
        if (!result)
        {
            ServerConnectFailedEvent?.Invoke(this, EventArgs.Empty);
            Log.Error("Server not available, please try again");
            return;
        }

        if (ResourceAddress != null)
            await UpdateCollection();
        //load list entries
        //load async image one by one
        // _client.Disconnect();
    }

    /// <summary>
    /// Update available items from resource address
    /// this is made virtual because not all item are legacy,
    /// so we override when needed
    /// </summary>
    private async Task UpdateCollection()
    {
        var itemsFolder = await _client.SftpServer.GetAllFilesAddressInFolder(ResourceAddress);
        if (itemsFolder == null)
        {
            Log.Information("Folder is empty: " + ResourceAddress);
            return;
        }
        
        Log.Information("Updating collection: " + ResourceAddress);
        foreach (var url in itemsFolder)
        {
            //get name
            if(!_client.SftpServer.IsFolder(url))
                continue;
            var itemName = _client.SftpServer.GetFileOrFolderName(url).Name;
            //get description content
            var descriptionPath = url + "/description.md";
            var description = await _client.SftpServer.GetStringContent(descriptionPath);
            var itemList = new List<OnlineItemModel>();
            var infoPath = url + "/info.json";
            var info = _client.SftpServer.GetFiles<OnlineItemModel>(infoPath).Result;
            info.Path = url;
            var item = Convert(info);
            item.ThumbnailPath = url + "/thumb.png";
            item.LastUpdate = _client.SftpServer.GetFileAttributes(infoPath).LastWriteTime;
            AddItem(item);
        }

        await GetFilters();
        Log.Information("Collection updated: Items count = " + itemsFolder.Count);
    }
    private async Task GetFilters()
    {
        var filterPath = ResourceAddress + "/filters.json";
        if(!_client.SftpServer.IsExist(filterPath))
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