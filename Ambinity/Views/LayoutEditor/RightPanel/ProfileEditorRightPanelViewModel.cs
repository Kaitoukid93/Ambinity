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

public class ProfileEditorRightPanelViewModel : ViewModelBase
{
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;

    public ProfileEditorRightPanelViewModel(LayersViewModel layersViewModel,
        Draw2DCanvasViewModel canvasViewModel, LibraryViewModelFactory libraryViewModelFactory
    )
    {
        LayersViewModel = layersViewModel;
        _canvasViewModel = canvasViewModel;
        _libraryViewModelFactory  = libraryViewModelFactory;
       
    }
    private void OnFigureRemoved(Figure obj)
    {
        PropertiesViewModel.DisableEdit();
    }
    public void OpenFlyout(FlyoutContentViewModelBase flyoutViewModel)
    {
        FlyoutViewModel = flyoutViewModel;
        OpenFlyoutEvent?.Invoke();
    }

    /// <summary>
    /// This is when user close by pressing button
    /// </summary>
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
    private Draw2DCanvasViewModel _canvasViewModel;

    public void Init()
    {
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
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
    private ViewModelBase _currentTabContent;
    private readonly LibraryViewModelFactory _libraryViewModelFactory;

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
                PropertiesViewModel.UpdateObjectProperties();
                CurrentTabContent = PropertiesViewModel;
                break;
            case 1:
                LayersViewModel.Update();
                CurrentTabContent = LayersViewModel;
                break;
        }
    }

    public override void Dispose()
    {
        //todo
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
    }
}