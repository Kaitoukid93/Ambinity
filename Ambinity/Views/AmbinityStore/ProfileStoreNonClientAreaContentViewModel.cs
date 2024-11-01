using System;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.OnlineStore.Library;
using Action = System.Action;

namespace Ambinity.Views.AmbinityStore;

public class ProfileStoreNonClientAreaContentViewModel : ViewModelBase

{
    private AssetsViewModelBase _assetsViewModel;

    public ProfileStoreNonClientAreaContentViewModel(LightingProfileAssetsViewModel assetsViewModel)
    {
        _assetsViewModel = assetsViewModel;
        _assetsViewModel.LoadingChanged += OnLoadingChanged;
        OnLoadingChanged();
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
            _assetsViewModel.SearchContent = value;
            OnPropertyChanged();
        }
    }
}