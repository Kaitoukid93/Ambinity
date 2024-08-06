using System.Text.Json.Serialization;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.Repositories;

public class ColorPalette : ObservableObject, ICollectableItem
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
        return Ioc.Default.GetRequiredService<ColorPaletteRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for color palette
        return null;
    }
    
    public string LocalPath { get; set; }
    public Color[] Colors { get; set; }

    public ColorPalette(string name, Color[] colors)
    {
        Colors = colors;
        Name = name;
    }

    public ColorPalette()
    {
    }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}