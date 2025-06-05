using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.Canvas;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Threading;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// UI logic for layers view, get updated when zone mapping or canvas update
/// </summary>
public class LayersViewModel : ViewModelBase
{
    public LayersViewModel(CanvasViewModelFactory canvasViewModelFactory)
    {
        canvasViewModelFactory.CurrentChanged += OnCanvasViewModelChanged;

        Layers = new ObservableCollection<LayerViewModel>();
    }

    private void OnCanvasViewModelChanged(CanvasViewModelBase vm)
    {
        if (_canvasViewModel != null)
        {
            _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
            _canvasViewModel.FigureAdded -= OnFigureAdded;
            _canvasViewModel.FigureRemoved -= OnFigureRemoved;
        }

        _canvasViewModel = vm;
        _canvasViewModel.SelectionChanged += OnCanvasSelectionChanged;
        _canvasViewModel.FigureAdded += OnFigureAdded;
        _canvasViewModel.FigureRemoved += OnFigureRemoved;
    }

    private void OnFigureRemoved(Figure figure)
    {
        //resolve figure and layer
        // Removelayer(figure);
    }

    private void OnFigureAdded(Figure figure)
    {
        //AddLayer(figure);
    }

    private void OnCanvasSelectionChanged()
    {
        UpdateLayerView();
    }

    private void UpdateLayerView()
    {
        foreach (var zone in Layers)
        {
            zone.Update();
        }
    }

    private CanvasViewModelBase _canvasViewModel;
    public ObservableCollection<LayerViewModel> Layers { get; set; }

    public async Task Update()
    {
        Layers.Clear();
        foreach (ContainerFigure figure in _canvasViewModel.Figures.Where(f => f is ContainerFigure && f.IsSelectable))
        {
            await Task.Run(() => AddLayer(figure));
            await Task.Delay(10);
        }
    }

    public void AddLayer(ContainerFigure figure)
    {
        var layer = new LayerViewModel(figure);
        layer.Selected += OnLayerSelected;
        Dispatcher.UIThread.InvokeAsync(() => Layers.Add(layer));
    }

    public void Removelayer(LayerViewModel layer)
    {
        layer.Selected -= OnLayerSelected;
        Layers.Remove(layer);
    }

    private void OnLayerSelected(LayerViewModel layer, bool isCtrl)
    {
        if (!isCtrl)
        {
            foreach (var l in Layers)
            {
                l.Figure.Unselect();
            }
        }

        if (!layer.Figure.IsSelected)
            layer.Figure.Select();
        else
        {
            layer.Figure.Unselect();
        }
    }

    public override void Dispose()
    {
        _canvasViewModel.SelectionChanged -= OnCanvasSelectionChanged;
        _canvasViewModel.FigureAdded -= OnFigureAdded;
        _canvasViewModel.FigureRemoved -= OnFigureRemoved;
    }
}
