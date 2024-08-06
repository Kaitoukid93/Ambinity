using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace AmbinityCore.Repositories;

public class GradientColor : StaticColor, ICollectableItem
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
    public GradientBrush Gradient { get; set; }

    public GradientColor(string name, GradientBrush brush)
    {
        Gradient = brush;
        Name = name;
    }

    public GradientColor()
    {
    }

    public override Brush GetBrush()
    {
        return Gradient;
    }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}