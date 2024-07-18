namespace AmbinityCore.Models.Collection;

public interface ICollectableItem
{
    event Action<ICollectableItem> ItemNameChanged;
    event Action<ICollectableItem> ItemPinStatusChanged;
    event Action<ICollectableItem> ItemCheckStatusChanged;
    string Name { get; set; }
    bool IsSelected { get; set; }
    bool IsEditing { get; set; }
    bool IsChecked { get; set; }
    bool IsPinned { get; set; }
    string LocalPath { get; set; }
}