using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for device layout editor view
/// </summary>
public class DeviceLayoutRightPanelViewModel : ViewModelBase
{
    public DeviceLayoutRightPanelViewModel(
        Draw2DCanvasViewModel canvasViewModel,
        RightPanelAssetsViewModel assetsesViewModel,
        LightingProfileDecoder decoder
    )
    {
        _decoder = decoder;
        _canvasViewModel = canvasViewModel;
        AssetsesViewModel = assetsesViewModel;
        
    }

    private void OnRenderingStatusChanged()
    {
        OnPropertyChanged(nameof(IsLocked));
    }


    /// <summary>
    /// This is when user close by pressing button
    /// </summary>
    private void OnFigureRemoved(Figure obj)
    {
        PropertiesViewModel.DisableEdit();
    }

    private void OnCanvasSelectionChanged()
    {
        PropertiesViewModel.UpdateObjectProperties();
    }
    
    public CanvasObjectPropertiesViewModelBase PropertiesViewModel { get; set; }
    public RightPanelAssetsViewModel AssetsesViewModel { get; set; }
    private Draw2DCanvasViewModel _canvasViewModel;
    private CollectableItemRepository _localRepository;
    private OnlineItemRepository _onlineItemRepository;
    private readonly LightingProfileDecoder _decoder;

    public async Task Init(CollectableItemRepository localRepo, OnlineItemRepository onlineRepo)
    {
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        _localRepository = localRepo;
        _onlineItemRepository = onlineRepo;
        PropertiesViewModel.Init();
        await AssetsesViewModel.Init(_localRepository, _onlineItemRepository);
    }

    public override void Dispose()
    {
        //todo
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
    }

    public bool IsLocked => _decoder.IsRendering;
}