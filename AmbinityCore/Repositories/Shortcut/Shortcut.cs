using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Repositories;

public class Shortcut : ObservableObject, ICollectableItem
{
    
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

    public OnlineItemRepository GetOnlineRepository()
    {
        //todo make online repo for color palette
        return null;
    }

    public string LocalPath { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }

    public Shortcut(string name)
    {
        Name = name;
    }

    public Shortcut()
    {
    }
    public Guid LightingProfileID { get; set; }
    public bool IsDefault { get; set; }
    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.Unknown;
    }

    public void Save()
    {
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = LocalRepository.LocalFolderPath;
            LocalPath = Path.Combine(dbPath, Name);
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "shortcut.json"));
    }

    public void Export(string path)
    {
        JsonHelpers.WriteSimpleJson(this, path);
    }
}