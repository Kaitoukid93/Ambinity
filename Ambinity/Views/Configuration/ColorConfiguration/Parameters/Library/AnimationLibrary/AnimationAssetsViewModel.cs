using Ambinity.Views.LayoutEditor;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class AnimationAssetsViewModel : AssetsViewModelBase
{
    public AnimationAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory) : base(downloadService, assetItemViewModelFactory)
    {
    }
    
}