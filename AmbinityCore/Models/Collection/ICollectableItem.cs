using System.ComponentModel;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Collection;

public interface ICollectableItem : INotifyPropertyChanged
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    string Thumbnail {get;}
}
