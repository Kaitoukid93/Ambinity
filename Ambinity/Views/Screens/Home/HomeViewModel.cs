using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.OnlineStore;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.Home;

public class HomeViewModel : ViewModelBase
{
    private readonly LightingProfileOnlineRepository _profileOnlineRepository;
    private readonly LightingProfileRepository _profileLocalRepository;
    private readonly DownloadService _downloadService;

    public HomeViewModel(LightingProfileOnlineRepository lightingProfileOnlineRepository, DownloadService downloadService, LightingProfileRepository lightingProfileRepository)
    {
        _downloadService = downloadService;
        _profileOnlineRepository = lightingProfileOnlineRepository;
        _profileLocalRepository = lightingProfileRepository;
        AvailableAssets = [];
        DisplayAssets = [];
    }

    public async Task Init()
    {
        AvailableAssets?.Clear();
        DisplayAssets?.Clear();
        await Task.Run(() => _profileOnlineRepository.Init());
        foreach (var item in _profileOnlineRepository.Items)
        {
            var asset = new OnlineItemAssetViewModel(item, _downloadService, _profileLocalRepository,150);
            RegisterAsset(asset);
            await Task.Run(() => Task.Delay(20));
            AvailableAssets.Add(asset);
        }

        foreach (var asset in AvailableAssets.Take(8))
        {
            DisplayAssets.Add(asset);
        }
    }
    private ObservableCollection<OnlineItemAssetViewModel> _availableAssets;

    public ObservableCollection<OnlineItemAssetViewModel> AvailableAssets
    {
        get => _availableAssets;
        set
        {
            _availableAssets = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<OnlineItemAssetViewModel> _displayAssets;
    public ObservableCollection<OnlineItemAssetViewModel> DisplayAssets
    {
        get => _displayAssets;
        set
        {
            _displayAssets = value;
            OnPropertyChanged();
        }
    }
    private void RegisterAsset(AssetItemViewModelBase asset)
    {
        asset.ItemSelected += OnAssetSelected;
    }

    private void OnAssetSelected(AssetItemViewModelBase obj)
    {
        // throw new System.NotImplementedException();
    }
}