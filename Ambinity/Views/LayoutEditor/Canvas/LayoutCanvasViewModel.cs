using System;
using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.ProfileEditor;
using AmbinityCore.DataBase;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using Avalonia;
using Draw2D.Core;
using Draw2D.Core.Graphic;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Policies.RouterPolicy;
using Draw2D.Core.Shapes.Basic;

namespace Ambinity.Views.LayoutEditor;

public class LayoutCanvasViewModel : ViewModelBase
{
    private readonly IGeneralSettings _generalSettings;
    private readonly FrameBuffer _buffer;
    public event Action<Figure> ItemAdded;
    public event Action<Figure> ItemRemoved;

    public LayoutCanvasViewModel(Draw2DCanvasViewModel canvasViewModel,
        ToolsViewModel toolsViewModel, Draw2DCanvasInfoBarViewModel infoBarViewModel, FrameBuffer buffer)
    {
        _buffer = buffer;
        CanvasViewModel = canvasViewModel;
        ToolsViewModel = toolsViewModel;
        InfoBarViewModel = infoBarViewModel;
       
    }

    private void InstallTool(PolylineTool tool)
    {
        CanvasViewModel.InstallPolylineTool();
    }

    private void OnFigureAddedFromTool(Figure figure)
    {
        CanvasViewModel.AddFigure(figure, true);
        figure.Select();
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
    public Draw2DCanvasInfoBarViewModel InfoBarViewModel { get; }

    public void Init(IEnumerable<IPositionAware> items)
    {
        //create canvas
        ToolsViewModel.FitCanvasToViewEvent += FitCanvasToView;
        ToolsViewModel.ToggleSnapToGridEvent += ToggleSnapToGrid;
        ToolsViewModel.InstallPolylineTool += InstallTool;
        ToolsViewModel.AddFigure += OnFigureAddedFromTool;
        CanvasViewModel.FigureAdded += OnFigureAdded;
        CanvasViewModel.FigureRemoved += OnFigureRemoved;
        CanvasViewModel.Init(new Size(_buffer.FrameWidth,_buffer.FrameHeight));
        CanvasViewModel.Canvas.ShouldDrawBackgroundImage = ShoudDrawBackground;
        //resolve list figures
        var zones = new List<ContainerFigure>();
        int zOrder = 0;
        foreach (var item in items)
        {
            var containerFigure = item.GetContainer();
            containerFigure.SetChild(item);
            containerFigure.ZOrder = zOrder++;
            CanvasViewModel.AddFigure(containerFigure, false);
            zones.Add(containerFigure);
        }
        InfoBarViewModel.Init();

    }

    public override void Dispose()
    {
        ToolsViewModel.FitCanvasToViewEvent -= FitCanvasToView;
        ToolsViewModel.ToggleSnapToGridEvent -= ToggleSnapToGrid;
        CanvasViewModel.FigureAdded -= OnFigureAdded;
        CanvasViewModel.FigureRemoved -= OnFigureRemoved;
        ToolsViewModel.InstallPolylineTool -= InstallTool;
        ToolsViewModel.AddFigure -= OnFigureAddedFromTool;
        CanvasViewModel?.Dispose();
        ToolsViewModel?.Dispose();
    }
}