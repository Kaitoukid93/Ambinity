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
    public event Action<ICollectableItem> ItemSelected;
    public LibraryViewModelBase(CollectableItemRepository localRepository,
        OnlineItemRepository onlineItemRepository,
        AssetsViewModelBase localAssetsViewModel)
    {
        _localRepository = localRepository;
        _onlineItemRepository = onlineItemRepository;
        AssetsViewModel = localAssetsViewModel;
        AssetsViewModel.ItemSelected += OnItemSelected;
        UpdateContent();
    }

    private void OnItemSelected(AssetItemViewModelBase obj)
    {
        ItemSelected?.Invoke(obj.Item);
    }

    private CollectableItemRepository _localRepository;
    private OnlineItemRepository _onlineItemRepository;
    public AssetsViewModelBase AssetsViewModel { get;}
    private ViewModelBase _currentTabContent;

    public ViewModelBase CurrentTabContent
    {
        get => _currentTabContent;
        set
        {
            _currentTabContent = value;
            OnPropertyChanged();
        }
    }

  

    public async Task Init()
    {
        await AssetsViewModel.Init(_localRepository, _onlineItemRepository);
    }

    private void UpdateContent()
    {
        CurrentTabContent = AssetsViewModel;
        AssetsViewModel.Init(_localRepository, _onlineItemRepository);
    }

    public override void Dispose()
    {
        AssetsViewModel?.Dispose();
    }
}