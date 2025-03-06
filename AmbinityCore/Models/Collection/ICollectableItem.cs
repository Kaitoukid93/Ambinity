using System.ComponentModel;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Collection;

public interface ICollectableItem : INotifyPropertyChanged
{
    event Action<ICollectableItem> ItemNameChanged;
    event Action<ICollectableItem> ItemPinStatusChanged;
    event Action<ICollectableItem> ItemCheckStatusChanged;
    string Name { get; set; }
    string Description { get; set; }
    bool IsSelected { get; set; }
    bool IsEditing { get; set; }
    bool IsChecked { get; set; }
    bool IsPinned { get; set; }
    string LocalPath { get; set; }
    OnlineItemTypeEnum GetType();
    void Save();

    /// <summary>
    /// get co-responding local repository for this item
    /// </summary>
    /// <returns></returns>
    CollectableItemRepository LocalRepository { get; set; }
    /// <summary>
    /// get co-responding online repository for this item
    /// </summary>
    /// <returns></returns>
    OnlineItemRepository GetOnlineRepository();
    
}