using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.AmbinityStore;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.OnlineStore;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;
using Task = System.Threading.Tasks.Task;

namespace Ambinity.Views.Screens.Home;

public class HomeViewModel : ViewModelBase
{
    private readonly LightingProfileOnlineRepository _profileOnlineRepository;
    private readonly LightingProfileRepository _profileLocalRepository;
    private readonly DownloadService _downloadService;
    private HomeViewModelFactory _factory;
    public event Action<AssetItemViewModelBase> ShowAllProfileRequested;

    public HomeViewModel(LightingProfileOnlineRepository lightingProfileOnlineRepository,
        DownloadService downloadService, LightingProfileRepository lightingProfileRepository, ProfileStoreViewModel profileStoreViewModel, HomeViewModelFactory factory,
        TutorialsOnlineRepository tutorialsOnlineRepository)
    {
        _tutorialRepository = tutorialsOnlineRepository;
        _profileStoreViewModel = profileStoreViewModel;
        _factory = factory;
        _downloadService = downloadService;
        _profileOnlineRepository = lightingProfileOnlineRepository;
        _profileLocalRepository = lightingProfileRepository;
        AvailableAssets = [];
        DisplayAssets = [];
        AvailableTutorials = [];
        ShowProfileLibraryCommand = new AsyncRelayCommand<AssetItemViewModelBase>(ShowProfileLibrary);
    }

    private async Task ShowProfileLibrary(AssetItemViewModelBase item = null)
    {
        ShowAllProfileRequested?.Invoke(item);
    }

    public async Task Init()
    {
        AvailableAssets?.Clear();
        AvailableTutorials?.Clear();
        DisplayAssets?.Clear();
        await Task.Run(() => _profileOnlineRepository.Init());
        foreach (var item in _profileOnlineRepository.Items)
        {
            var asset = new OnlineItemAssetViewModel(item, _downloadService, _profileLocalRepository, 300);
            RegisterAsset(asset);
            await Task.Run(() => Task.Delay(20));
            AvailableAssets.Add(asset);
        }

        foreach (var asset in AvailableAssets.Take(8))
        {
            DisplayAssets.Add(asset);
            asset.ItemSelected += OnProfileSelected;
        }
         //load tutorials if any
        _tutorialRepository.Init();
        foreach (var item in _tutorialRepository.Items)
        {
            AvailableTutorials.Add(_factory.GetHyperLinkViewModel(item));
        }
       
    }

    private async void OnProfileSelected(AssetItemViewModelBase item)
    {
        ShowAllProfileRequested?.Invoke(item);
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

    private ObservableCollection<HyperLinkHomeViewModel> _availableTutorials;

    public ObservableCollection<HyperLinkHomeViewModel> AvailableTutorials
    {
        get => _availableTutorials;
        set
        {
            _availableTutorials = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<OnlineItemAssetViewModel> _displayAssets;
    private readonly IWindowService _windowService;
    private readonly ProfileStoreViewModel _profileStoreViewModel;
    private readonly TutorialsOnlineRepository _tutorialRepository;
    private readonly RootNavigationStores _rootNavigationStore;

    public ObservableCollection<OnlineItemAssetViewModel> DisplayAssets
    {
        get => _displayAssets;
        set
        {
            _displayAssets = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowProfileLibraryCommand { get; }

    private void RegisterAsset(AssetItemViewModelBase asset)
    {
        asset.ItemSelected += OnAssetSelected;
    }

    private void OnAssetSelected(AssetItemViewModelBase obj)
    {
        // throw new System.NotImplementedException();
    }
}