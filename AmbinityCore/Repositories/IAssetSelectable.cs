using AmbinityCore.Models.Collection;

namespace AmbinityCore.Repositories;
/// <summary>
/// representing items that have property can change by selecting asset
/// </summary>
public interface IAssetSelectable
{
    ICollectableItem AssetSelectableProperty { get; }
}