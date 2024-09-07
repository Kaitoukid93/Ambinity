using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.OnlineStore;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for asset tab that contains local and online assets
/// </summary>
public abstract class AssetsViewModelBase : ViewModelBase
{
    public AssetsViewModelBase(DownloadService downloadService, AssetItemViewModelFactory assetItemViewModelFactory)
    {
        _assetItemViewModelFactory = assetItemViewModelFactory;
        _downloadService = downloadService;
    }

    private string[] _filters;

    public string[] Filters
    {
        get => _filters;
        set
        {
            _filters = value;
            OnPropertyChanged();
        }
    }

  
    public event Action<AssetItemViewModelBase> ItemSelected;

    private ObservableCollection<AssetItemViewModelBase> _displayAssets;
    private DownloadService _downloadService;

    public ObservableCollection<AssetItemViewModelBase> DisplayAssets
    {
        get => _displayAssets;
        set
        {
            _displayAssets = value;
            OnPropertyChanged();
        }
    }

    private CollectableItemRepository _localRerpository;
    private OnlineItemRepository _onlineItemRepository;
    private ObservableCollection<AssetItemViewModelBase> _availableAssets;

    public ObservableCollection<AssetItemViewModelBase> AvailableAssets
    {
        get => _availableAssets;
        set
        {
            _availableAssets = value;
            OnPropertyChanged();
        }
    }

    private string _searchContent;

    public string SearchContent
    {
        get => _searchContent;
        set
        {
            _searchContent = value;
            FilterItem(value);
            OnPropertyChanged();
        }
    }

    private bool _isloading;

    public bool IsLoading
    {
        get => _isloading;
        set
        {
            _isloading = value;
            OnPropertyChanged();
        }
    }

    private string _selectedRepository;
    private readonly AssetItemViewModelFactory _assetItemViewModelFactory;

    public string SelectedRepository
    {
        get => _selectedRepository;
        set
        {
            if (_selectedRepository == value)
                return;
            _selectedRepository = value;
            OnPropertyChanged();
            UpdateAssets();
        }
    }

    private List<string> _tabs;
    

    public List<string> Tabs
    {
        get => _tabs;
        set
        {
            _tabs = value;
            OnPropertyChanged();
        }
    }

    public bool ShowNoResult => !IsLoading && DisplayAssets.Count == 0;

    public async Task UpdateAssets()
    {
        if(IsLoading)
            return;
        IsLoading = true;
        DisplayAssets?.Clear();
        AvailableAssets?.Clear();
        SearchContent = string.Empty;
        switch (_selectedRepository)
        {
            case "Local":
                foreach (var item in _localRerpository.Items)
                {
                    await Task.Run(() => Task.Delay(20));
                    var asset = _assetItemViewModelFactory.GetViewModel(item);
                    RegisterAsset(asset);
                    AvailableAssets.Add(asset);
                    DisplayAssets.Add(asset);
                }

                break;
            case "Online":
                await Task.Run(() => _onlineItemRepository.Init());
                foreach (var item in _onlineItemRepository.Items)
                {
                    var asset = new OnlineItemAssetViewModel(item, _downloadService, _localRerpository);
                    RegisterAsset(asset);
                    await Task.Run(() => Task.Delay(20));
                    AvailableAssets.Add(asset);
                    DisplayAssets.Add(asset);
                }

                Filters = _onlineItemRepository.Filters.ToArray();
                //download filter
                
                break;
        }

        IsLoading = false;
    }

    public async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        _localRerpository = localRepo;
        _onlineItemRepository = onlineRepo;
        DisplayAssets = new ObservableCollection<AssetItemViewModelBase>();
        AvailableAssets = new ObservableCollection<AssetItemViewModelBase>();
        Tabs = ["Local", "Online"];
        //show local repo
        _selectedRepository = "Local";
        await UpdateAssets();
    }

    private void UnRegisterAsset(AssetItemViewModelBase asset)
    {
        asset.ItemSelected -= OnAssetSelected;
    }
    private void FilterItem(string filter)
    {
        var searchText = filter.ToLower();
        var filteredItems = AvailableAssets
            .Where(item => item.Name.ToLower().Contains(searchText))
            .ToList();
        DisplayAssets.Clear();
        foreach (var asset in filteredItems)
        {
            DisplayAssets.Add(asset);
        }

        OnPropertyChanged(nameof(ShowNoResult));
    }

    private void RegisterAsset(AssetItemViewModelBase asset)
    {
        asset.ItemSelected += OnAssetSelected;
    }

    private void OnAssetSelected(AssetItemViewModelBase obj)
    {
        ItemSelected?.Invoke(obj);
    }

    public virtual void Dispose()
    {
        AvailableAssets = null;
        DisplayAssets = null;
    }
}