using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.OnlineStore.Library;
using CommunityToolkit.Mvvm.Input;
using Action = System.Action;

namespace Ambinity.Views.AmbinityStore;

public class ProfileStoreNonClientAreaContentViewModel : ViewModelBase

{
    private AssetsViewModelBase _assetsViewModel;

    public ProfileStoreNonClientAreaContentViewModel(LightingProfileAssetsViewModel assetsViewModel,
        AmbinityStoreNavigation navigation)
    {
        _navigation = navigation;
        _navigation.CurrentViewModelChanged += OnCurrentViewModelChanged;
        _assetsViewModel = assetsViewModel;
        _assetsViewModel.LoadingChanged += OnLoadingChanged;
        OnLoadingChanged();
        SearchCommand = new AsyncRelayCommand(SearchItem, CanSearch);
    }

    private async Task SearchItem()
    {
        await _assetsViewModel.SearchItem(SearchContent);
    }

    private bool CanSearch()
    {
        return SearchContent != null || SearchContent != string.Empty;
    }

    private void OnCurrentViewModelChanged(ViewModelBase vm)
    {
        SearchEnable = vm is not AmbinityStoreDetailViewModel;
    }

    private void OnLoadingChanged()
    {
        OnPropertyChanged(nameof(EnableSearchBar));
    }


    public bool EnableSearchBar => !_assetsViewModel.IsLoading;
    private string _searchContent;

    public string SearchContent
    {
        get => _searchContent;
        set
        {
            _searchContent = value;
            OnPropertyChanged();
            SearchCommand.NotifyCanExecuteChanged();
        }
    }

    private bool _searchEnable;
    private readonly AmbinityStoreNavigation _navigation;

    public bool SearchEnable
    {
        get => _searchEnable;
        set
        {
            _searchEnable = value;
            OnPropertyChanged();
        }
    }

    public AsyncRelayCommand SearchCommand { get; }
}