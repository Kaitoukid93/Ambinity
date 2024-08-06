using System;
using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.ProfileEditor;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using Avalonia;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor;

public class LayoutCanvasViewModel : ViewModelBase
{
    public event Action<Figure> ItemAdded;
    public event Action<Figure> ItemRemoved;

    public LayoutCanvasViewModel(Draw2DCanvasViewModel canvasViewModel,
        ToolsViewModel toolsViewModel)
    {
        CanvasViewModel = canvasViewModel;
        ToolsViewModel = toolsViewModel;
        ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
        ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
        CanvasViewModel.FigureAdded += OnFigureAdded;
        CanvasViewModel.FigureRemoved += OnFigureRemoved;
    }

    public bool ShoudDrawBackground { get; set; }

    private void OnFigureRemoved(Figure figure)
    {
        ItemRemoved?.Invoke(figure);
    }

    private void OnFigureAdded(Figure figure)
    {
        ItemAdded?.Invoke(figure);
    }

    private void ToggleSnapToGrid()
    {
        CanvasViewModel.ToggleGridSnapCommand.Execute(null);
    }

    private void FitCanvasToView()
    {
        CanvasViewModel.FitCommand.Execute(null);
    }

    // lock resize and drag
    public Draw2DCanvasViewModel CanvasViewModel { get; }
    public ToolsViewModel ToolsViewModel { get; }

    public void Init(IEnumerable<IPositionAware> items)
    {
        //create canvas
        CanvasViewModel.Init(new Size(500, 250));
        CanvasViewModel.Canvas.ShouldDrawBackgroundImage = ShoudDrawBackground;
        //resolve list figures
        foreach (var item in items)
        {
            var containerFigure = item.GetContainer();
            containerFigure.SetChild(item);
            CanvasViewModel.AddFigure(containerFigure);
        }

        //create tools 
        ToolsViewModel.Init();
    }

    public override void Dispose()
    {
        ToolsViewModel.FitCanvasToViewEvent -= FitCanvasToView;
        ToolsViewModel.ToggleSnapToGridEvent -= ToggleSnapToGrid;
        CanvasViewModel.FigureAdded -= OnFigureAdded;
        CanvasViewModel.FigureRemoved -= OnFigureRemoved;
        CanvasViewModel.Dispose();
        ToolsViewModel.Dispose();
    }
}