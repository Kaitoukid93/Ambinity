using System.Collections.ObjectModel;
using System.Linq;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Lighting.Zone;

namespace Ambinity.Views.Screens.ProfileEditor;

/// <summary>
/// UI logic for layers view, get updated when zone mapping or canvas update
/// </summary>
public class LayersViewModel
{
    public LayersViewModel(Draw2DCanvasViewModel canvasViewModel)
    {
        _canvasViewModel = canvasViewModel;
        Layers = new ObservableCollection<LayerViewModelBase>();
    }

    private Draw2DCanvasViewModel _canvasViewModel;
    public ObservableCollection<LayerViewModelBase> Layers { get; set; }

    public void Init()
    {
    }

    public void Update()
    {
        Layers.Clear();
        foreach (var figure in _canvasViewModel.Figures.Where(f=>f is LightingZoneFigure))
        {
            var layer = new ZoneLayerViewModel(figure as LightingZoneFigure);
            Layers.Add(layer);
        }
    }
}