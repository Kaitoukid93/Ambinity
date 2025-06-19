using System;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor.Canvas;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using Draw2D.Core.Shapes.Basic;
using Microsoft.Extensions.DependencyInjection;
using Canvas = Draw2D.Core.Canvas;

namespace Ambinity.Views.Draw2DCanvas;

public partial class Draw2DCanvasView : UserControl
{
    private CanvasViewModelFactory _viewModelFactory;
    private FigureContextMenuProvider _contextMenuProvider;
    private CanvasViewModelBase _viewModel;

    public Draw2DCanvasView()
    {
        InitializeComponent();
        _viewModelFactory = Ioc.Default.GetRequiredService<CanvasViewModelFactory>();
        _contextMenuProvider = Ioc.Default.GetRequiredService<FigureContextMenuProvider>();
        _viewModelFactory.CurrentChanged += OnCurrentCanvasViewModelChanged;
        
    }

    private void OnCurrentCanvasViewModelChanged(CanvasViewModelBase vm)
    {
        _viewModel = vm;
    }

    private void OnCanvasRightClicked(object? sender, CanvasClickEventArgs e)
    {
        if (_viewModel.IsLocked)
            return;
        var point = new Point(e.MousePosX, e.MousePosY);
        var menu = _contextMenuProvider.GetContextMenu(null, point);
        menu.ShowAt(Draw2DControl);
    }

    private void OnFigureRightClicked(object? sender, FigureClickEventArgs e)
    {
        if (_viewModel.IsLocked)
            return;
        var figure = e.Sender as Figure;
        var point = new Point(e.MousePosX, e.MousePosY);
        var menu = _contextMenuProvider.GetContextMenu(figure, point);
        menu.ShowAt(Draw2DControl);
    }
}
