using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.DeviceLayout.Library;

public class DeviceLayoutAssetsViewModel : AssetsViewModelBase
{
    private readonly IWindowService _windowService;
    public DeviceLayoutAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory, IWindowService windowService) : base(downloadService, assetItemViewModelFactory)
    {
        // ImportPaletteCommand = new AsyncRelayCommand(ImportPalette);

        _windowService = windowService;
    }
    public override async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        await base.Init(localRepo, onlineRepo);
        //for now just show online repo
        SelectedRepository = "Local";
    }
    //  public ICommand ImportPaletteCommand { get; set; }
}