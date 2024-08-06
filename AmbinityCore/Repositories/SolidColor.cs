using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.Repositories;

public class SolidColor : StaticColor, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public bool IsEditing { get; set; }
    public bool IsChecked { get; set; }
    public bool IsPinned { get; set; }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<StaticColorsRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for solid color 
        return null;
    }

    public string LocalPath { get; set; }
    public Color Color { get; set; }

    public SolidColor(string name, Color color)
    {
        Color = color;
        Name = name;
    }

    public override Brush GetBrush()
    {
        return new SolidColorBrush(Color);
    }

    public SolidColor()
    {
    }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}