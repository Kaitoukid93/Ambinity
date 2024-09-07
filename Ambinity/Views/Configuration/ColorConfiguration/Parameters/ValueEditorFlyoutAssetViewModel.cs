using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;
public class ValueEditorFlyoutAssetViewModel : AssetsViewModelBase
{
    public ValueEditorFlyoutAssetViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory) : base(downloadService, assetItemViewModelFactory)
    {
    }
}