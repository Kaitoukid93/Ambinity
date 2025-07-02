using Ambinity.Views.Draw2DCanvas;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor.Canvas;

public partial class Draw2DCanvasView : UserControl
{
    private CanvasViewModelFactory _canvasViewModelFactory;
    private CanvasViewModelBase? _viewModel;
    private FigureContextMenuProvider _contextMenuProvider;

    public Draw2DCanvasView()
    {
        InitializeComponent();
        _canvasViewModelFactory = Ioc.Default.GetRequiredService<CanvasViewModelFactory>();
        _viewModel = _canvasViewModelFactory.Current;
        _viewModel.Canvas.FigureRightClicked += OnFigureRightClicked;
        _viewModel.Canvas.CanvasRightClicked += OnCanvasRightClicked;
        _contextMenuProvider = Ioc.Default.GetRequiredService<FigureContextMenuProvider>();
    }
       private void OnCanvasRightClicked(object? sender, CanvasClickEventArgs e)
    {
        if (_viewModel.IsLocked)
            return;
        var point = new Point(e.MousePosX, e.MousePosY);
        _contextMenuProvider.CanvasVM = _viewModel;
        _contextMenuProvider.Init();
        var menu = _contextMenuProvider.GetContextMenu(null, point);
        menu.ShowAt(Draw2DControl);
    }

    private void OnFigureRightClicked(object? sender, FigureClickEventArgs e)
    {
        if (_viewModel.IsLocked)
            return;
        _contextMenuProvider.CanvasVM = _viewModel;
        _contextMenuProvider.Init();
        var figure = e.Sender as Figure;
        var point = new Point(e.MousePosX, e.MousePosY);
        var menu = _contextMenuProvider.GetContextMenu(figure, point);
        menu.ShowAt(Draw2DControl);
    }
}
