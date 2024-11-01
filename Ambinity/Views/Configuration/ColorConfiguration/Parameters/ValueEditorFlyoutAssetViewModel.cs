using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;
public class ValueEditorFlyoutAssetViewModel : AssetsViewModelBase
{
    public ValueEditorFlyoutAssetViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory) : base(downloadService, assetItemViewModelFactory)
    {
    }
    public override async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        await base.Init(localRepo, onlineRepo);
        //for now just show online repo
        SelectedRepository = "Local";
    }
}