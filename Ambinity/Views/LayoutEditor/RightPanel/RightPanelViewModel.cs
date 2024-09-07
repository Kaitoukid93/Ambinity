using System;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

public class RightPanelViewModel : ViewModelBase
{
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;

    public RightPanelViewModel(LayersViewModel layersViewModel,
        Draw2DCanvasViewModel canvasViewModel,
        RightPanelAssetsViewModel assetsesViewModel
    )
    {
        LayersViewModel = layersViewModel;
        _canvasViewModel = canvasViewModel;
        AssetsesViewModel = assetsesViewModel;
       
    }

    public void OpenFlyout(FlyoutContentViewModelBase flyoutViewModel)
    {
        FlyoutViewModel = flyoutViewModel;
        OpenFlyoutEvent?.Invoke();
    }

    /// <summary>
    /// This is when user close by pressing button
    /// </summary>
    private void OnFigureRemoved(Figure obj)
    {
        PropertiesViewModel.DisableEdit();
    }

    public void OnFlyoutClosing()
    {
        FlyoutViewModel.Dispose();
        FlyoutViewModel = null;
    }

    private void OnCanvasSelectionChanged()
    {
        PropertiesViewModel.UpdateObjectProperties();
    }

    public FlyoutContentViewModelBase FlyoutViewModel { get; set; }
    public CanvasObjectPropertiesViewModelBase PropertiesViewModel { get; set; }
    public LayersViewModel LayersViewModel { get; set; }
    public RightPanelAssetsViewModel AssetsesViewModel { get; set; }
    private Draw2DCanvasViewModel _canvasViewModel;
    private CollectableItemRepository _localRepository;
    private OnlineItemRepository _onlineItemRepository;

    public void Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
        _localRepository = localRepo;
        _onlineItemRepository = onlineRepo;
        PropertiesViewModel.Init();
        SelectedTab = 0;
    }

    private int _selectedTab;

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            OnPropertyChanged();
            UpdateTabContent();
        }
    }

    private bool _isFlyoutOpen;

    public bool IsFlyoutOpen
    {
        get => _isFlyoutOpen;
        set
        {
            _isFlyoutOpen = value;
            OnPropertyChanged();
        }
    }

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

    public async Task UpdateTabContent()
    {
        switch (SelectedTab)
        {
            case 0:
                PropertiesViewModel.Init();
                CurrentTabContent = PropertiesViewModel;
                break;
            case 1:
                LayersViewModel.Update();
                CurrentTabContent = LayersViewModel;
                break;
            case 2:
                CurrentTabContent = AssetsesViewModel;
                await AssetsesViewModel.Init(_localRepository, _onlineItemRepository);
                break;
        }
    }

    public override void Dispose()
    {
        //todo
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
    }
}