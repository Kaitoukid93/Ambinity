using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.OnlineStore.Library;

public class LightingProfileAssetsViewModel : AssetsViewModelBase
{
    public LightingProfileAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory) : base(downloadService, assetItemViewModelFactory)
    {
    }

    private int _itemsRows;

    public int ItemsRows
    {
        get => _itemsRows;
        set
        {
            _itemsRows = value;
            OnPropertyChanged();
        }
    }

    public override async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        await base.Init(localRepo, onlineRepo);
        //for now just show online repo
        await UpdateAssets("Online",true);
    }

    public async Task FilterItem(string[] filters)
    {
        //await UpdateAssets("Online",true);
        DisplayAssets?.Clear();
        SearchContent = null;
        foreach (var item in AvailableAssets)
        {
            if (item is OnlineItemAssetViewModel onlineAsset)
            {
                if (filters.Intersect(onlineAsset.OnlineItemData.Tags).Any())
                    DisplayAssets.Add(item);
            }
        }
        OnPropertyChanged(nameof(ShowNoResult));
    }
}