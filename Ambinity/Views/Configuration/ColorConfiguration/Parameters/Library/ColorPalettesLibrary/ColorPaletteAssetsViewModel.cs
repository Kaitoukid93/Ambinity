using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Views.LayoutEditor;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ColorPaletteAssetsViewModel : AssetsViewModelBase
{
    private readonly IWindowService _windowService;
    public ColorPaletteAssetsViewModel(DownloadService downloadService,
        AssetItemViewModelFactory assetItemViewModelFactory, IWindowService windowService) : base(downloadService, assetItemViewModelFactory)
    {
       // ImportPaletteCommand = new AsyncRelayCommand(ImportPalette);

        _windowService = windowService;
    }
    
  //  public ICommand ImportPaletteCommand { get; set; }
}