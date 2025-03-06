using AmbinityCore.Colors;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;

namespace AmbinityCore.Repositories;

public class SolidColor : FillColorBase, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSelected { get; set; }
    public bool IsEditing { get; set; }
    public bool IsChecked { get; set; }
    public bool IsPinned { get; set; }

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

    public OnlineItemRepository GetOnlineRepository()
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

    public override List<Brush>  GetBrush()
    {
        return new List<Brush>() { new SolidColorBrush(Color) };
    }

    public SolidColor()
    {
    }

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
            LocalPath = Path.Combine(dbPath, Name + ".json"); // item without thumbnaill will be store in the same folder
        }
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}