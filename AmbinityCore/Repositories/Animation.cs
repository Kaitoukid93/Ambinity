using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;

namespace AmbinityCore.Repositories;

public class Animation : ObservableObject, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<AnimationsRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for color palette
        return null;
    }

    public string LocalPath { get; set; }

    public Animation(string name)
    {
        Name = name;
    }

    public Animation()
    {
    }

    /// <summary>
    /// load animation frame into ram
    /// </summary>
    public void LoadAnimation()
    {
        
    }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }

    public void Export(string path)
    {
        JsonHelpers.WriteSimpleJson(this, path);
    }
}