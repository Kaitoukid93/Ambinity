using Ambinity.Services;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.LayoutEditor;

public class RightPanelAssetsViewModel : AssetsViewModelBase
{
    public RightPanelAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory) : base(downloadService, assetItemViewModelFactory)
    {
        
    }
   

  
}
