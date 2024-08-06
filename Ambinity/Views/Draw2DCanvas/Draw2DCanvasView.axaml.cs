using System.Windows.Input;
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

namespace Ambinity.Views.Draw2DCanvas;

public partial class Draw2DCanvasView : UserControl
{
    private Draw2DCanvasViewModel _viewModel;
    private FigureContextMenuProvider _contextMenuProvider;

    public Draw2DCanvasView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<Draw2DCanvasViewModel>();
        _contextMenuProvider = Ioc.Default.GetRequiredService<FigureContextMenuProvider>();
        _viewModel.Canvas.FigureRightClicked += OnFigureRightClicked;
        _viewModel.Canvas.CanvasRightClicked += OnCanvasRightClicked;
    }
    private void OnCanvasRightClicked(object? sender, CanvasClickEventArgs e)
    {
        var point = new Point(e.MousePosX, e.MousePosY);
        var menu = _contextMenuProvider.GetContextMenu(null, point);
        menu.ShowAt(Draw2DControl);
    }

    private void OnFigureRightClicked(object? sender, FigureClickEventArgs e)
    {
        var figure = e.Sender as Figure;
        var point = new Point(e.MousePosX, e.MousePosY);
        var menu = _contextMenuProvider.GetContextMenu(figure, point);
        menu.ShowAt(Draw2DControl);
    }
}