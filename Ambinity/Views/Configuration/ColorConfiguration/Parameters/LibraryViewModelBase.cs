using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class LibraryViewModelBase : FlyoutContentViewModelBase
{
    
    /// <summary>
    /// Library base viewmodel for all collectable item that require a library view
    /// Requirements:
    /// Local Repository: repository for local storage
    /// Online Repository : repository for seeking online item
    /// AssetsViewModel : the viewmodel for displaying tabs, handling tab switch and display logic
    /// todo DetailsViewModel: detail when click item
    /// </summary>
    public event Action<AssetItemViewModelBase> ItemSelected;
    public LibraryViewModelBase(CollectableItemRepository localRepository,
        OnlineItemRepository onlineItemRepository,
        AssetsViewModelBase localAssetsViewModel)
    {
        _localRepository = localRepository;
        _onlineItemRepository = onlineItemRepository;
        AssetsViewModel = localAssetsViewModel;
        AssetsViewModel.ItemSelected += OnItemSelected;
        
    }

    private void OnItemSelected(AssetItemViewModelBase obj)
    {
        ItemSelected?.Invoke(obj);
    }

    private CollectableItemRepository _localRepository;
    private OnlineItemRepository _onlineItemRepository;
    public AssetsViewModelBase AssetsViewModel { get;}
    private ViewModelBase _currentViewContent;

    public ViewModelBase CurrentViewContent
    {
        get => _currentViewContent;
        set
        {
            _currentViewContent = value;
            OnPropertyChanged();
        }
    }

  

    public async Task Init()
    {
        CurrentViewContent = AssetsViewModel;
        await AssetsViewModel.Init(_localRepository, _onlineItemRepository);
        
    }

    private void UpdateContent()
    {
        CurrentViewContent = AssetsViewModel;
        AssetsViewModel.Init(_localRepository, _onlineItemRepository);
    }

    public override void Dispose()
    {
        AssetsViewModel?.Dispose();
        ItemSelected = null;
    }
}