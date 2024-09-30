using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.ProfileEditor.Library;

public class LightingZoneAssetsViewModel : AssetsViewModelBase
{
    private readonly IWindowService _windowService;
    public LightingZoneAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory, IWindowService windowService) : base(downloadService, assetItemViewModelFactory)
    {
        // ImportPaletteCommand = new AsyncRelayCommand(ImportPalette);

        _windowService = windowService;
    }
    
    //  public ICommand ImportPaletteCommand { get; set; }
}