using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.OnlineStore;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for asset tab that contains local and online assets
/// </summary>
public class AssetsViewModelBase : ViewModelBase
{
    public AssetsViewModelBase()
    {
        DisplayAssets = new ObservableCollection<AssetItemViewModelBase>();
        AvailableAssets = new ObservableCollection<AssetItemViewModelBase>();
    }
    
    private ObservableCollection<AssetItemViewModelBase> _displayAssets;

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
    public ObservableCollection<AssetItemViewModelBase> AvailableAssets { get; set; }
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

    private int _selectedRepository;

    public int SelectedRepository
    {
        get => _selectedRepository;
        set
        {
            _selectedRepository = value;
            OnPropertyChanged();
            UpdateAssets(value);
        }
    }

    public bool ShowNoResult => !IsLoading && DisplayAssets.Count == 0;

    public async Task UpdateAssets(int selectedRepo)
    {
        IsLoading = true;
        DisplayAssets?.Clear();
        AvailableAssets?.Clear();
        SearchContent = string.Empty;
        switch (selectedRepo)
        {
            case 0:
                foreach (var item in _localRerpository.Items)
                {
                    await Task.Run(() => Task.Delay(20));
                    var asset = AssetItemViewModelFactory.GetViewModel(item);
                    AvailableAssets.Add(asset);
                    DisplayAssets.Add(asset);
                }

                break;
            case 1:
                await Task.Run(() => _onlineItemRepository.Init());
                foreach (var item in _onlineItemRepository.Items)
                {
                    var asset = new OnlineItemAssetViewModel(item);
                    asset.IsLocalExisted = _localRerpository.Items.Any(i => i.Name == asset.Name);
                    await Task.Run(() => Task.Delay(20));
                    AvailableAssets.Add(asset);
                    DisplayAssets.Add(asset);
                }

                break;
        }

        IsLoading = false;
    }

    public async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        _localRerpository = localRepo;
        _onlineItemRepository = onlineRepo;
        //show local repo
        SelectedRepository = 0;
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
}