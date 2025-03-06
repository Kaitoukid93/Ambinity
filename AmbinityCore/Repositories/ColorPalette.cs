using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using Newtonsoft.Json;

namespace AmbinityCore.Repositories;

public class ColorPalette : FillColorBase, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public string Description { get; set; }
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

    [JsonIgnore] public string LocalPath { get; set; }
    public Color[] Colors { get; set; }

    public override List<Brush> GetBrush()
    {
        var brushes = new List<Brush>();
        foreach (var color in Colors)
        {
            brushes.Add(new SolidColorBrush(color));
        }

        return brushes;
    }

    public ColorPalette(string name, Color[] colors)
    {
        Colors = colors;
        Name = name;
    }

    public ColorPalette()
    {
    }

    public ColorPalette(Color[] colors)
    {
        Colors = colors;
    }

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.ColorPalette;
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

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "config.json"), new HexColorConverter());
    }

    public Color[] Resize(int numColor)
    {
        int w1 = Colors.Length;
        int w2 = numColor;
        Color[] temp = new Color[8];
        int x_ratio = (int)((w1 << 16) / w2) + 1;
        int y_ratio = 1;
        int x2, y2;
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < w2; j++)
            {
                x2 = ((j * x_ratio) >> 16);
                y2 = ((i * y_ratio) >> 16);
                temp[(i * w2) + j] = Colors[(y2 * w1) + x2];
            }
        }

        return temp;
    }
}