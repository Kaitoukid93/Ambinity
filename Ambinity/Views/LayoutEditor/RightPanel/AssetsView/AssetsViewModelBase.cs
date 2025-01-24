using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.OnlineStore;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for asset tab that contains local and online assets
/// </summary>
public abstract class AssetsViewModelBase : ViewModelBase
{
    public event Action LoadingChanged;

    public AssetsViewModelBase(DownloadService downloadService, AssetItemViewModelFactory assetItemViewModelFactory)
    {
        _assetItemViewModelFactory = assetItemViewModelFactory;
        _downloadService = downloadService;
        Tabs = ["Local", "Online"];
        DisplayAssets = [];
        AvailableAssets = [];
        SearchCommand = new AsyncRelayCommand<string>(SearchItem);
    }

    public async Task SearchItem(string content)
    {
        SearchContent = content;
        await FilterItem();
    }

    public ICommand SearchCommand { get; set; }
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
            LoadingChanged?.Invoke();
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
            _selectedRepository = value;
            OnPropertyChanged();
            UpdateAssets(_selectedRepository);
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

    public async Task UpdateAssets(string repo = null, bool isBackground = false)
    {
        IsLoading = true;
        DisplayAssets?.Clear();
        AvailableAssets?.Clear();
        _searchContent = string.Empty;
        switch (repo)
        {
            case "Local":
                //for local repo, load all items available
                foreach (var item in _localRerpository.Items)
                {
                    await Task.Run(() => Task.Delay(20));
                    var asset = _assetItemViewModelFactory.GetViewModel(item);
                    if (asset == null)
                    {
                        continue;
                    }

                    RegisterAsset(asset);
                    AvailableAssets.Add(asset);
                    if (!isBackground)
                        DisplayAssets.Add(asset);
                    //todo filter? at the moment, download items is too small in quantity to actually need a filter system
                }

                break;
            case "Online":
                _selectedRepository = repo;
                //for online repo, display items will be update each time Items get updated
                await Task.Run(() => _onlineItemRepository.Init());
                Filters = _onlineItemRepository.Filters.ToArray();
                break;
        }

        IsLoading = false;
    }

    public async Task LoadMoreIfAvailable()
    {
        if (_selectedRepository == "Local")
            return;
        IsLoading = true;
        await _onlineItemRepository.UpdateCollection(SearchContent.ToLower());
        IsLoading = false;
    }

    public virtual async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        _localRerpository = localRepo;
        _onlineItemRepository = onlineRepo;

        _onlineItemRepository.Items.CollectionChanged += OnItemsCollectionChanged;
        DisplayAssets = [];
        AvailableAssets = [];
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null)
        {
            AvailableAssets?.Clear();
            DisplayAssets?.Clear();
            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (OnlineItem item in e.NewItems)
            {
                var asset = new OnlineItemAssetViewModel(item, _downloadService, _localRerpository);
                RegisterAsset(asset);
                AvailableAssets.Add(asset);
                DisplayAssets.Add(asset);
                Log.Information("Item found: " + item.Name);
                Log.Information("Display Item Count: " + DisplayAssets.Count);
            }
        }
    }

    private async Task FilterItem()
    {
        //suppressor for user rapid input using delay in textbox axaml
        IsLoading = true;
        var searchText = SearchContent.ToLower();
        var filteredItems = new List<AssetItemViewModelBase>();
        switch (_selectedRepository)
        {
            case "Local":
                filteredItems = AvailableAssets
                    .Where(item => item.Name.ToLower().Contains(searchText))
                    .ToList();
                DisplayAssets.Clear();
                foreach (var asset in filteredItems)
                {
                    DisplayAssets.Add(asset);
                }

                break;
            case "Online":
                await Task.Run(() => _onlineItemRepository.UpdateCollection(searchText));
                break;
        }

        OnPropertyChanged(nameof(ShowNoResult));
        IsLoading = false;
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
        IsLoading = false;
        AvailableAssets = null;
        DisplayAssets = null;
        _onlineItemRepository.Items.CollectionChanged -= OnItemsCollectionChanged;
    }
}