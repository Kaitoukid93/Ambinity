using AmbinityCore.Models.Collection;
using Avalonia.Media;

namespace AmbinityCore.Repositories;

public class SolidColor : ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public bool IsEditing { get; set; }
    public bool IsChecked { get; set; }
    public bool IsPinned { get; set; }
    public string LocalPath { get; set; }
    public Color Color { get; set; }
    public SolidColor(string name,Color color)
    {
        Color = color;
        Name = name;
    }

    public SolidColor()
    {
        
    }
}