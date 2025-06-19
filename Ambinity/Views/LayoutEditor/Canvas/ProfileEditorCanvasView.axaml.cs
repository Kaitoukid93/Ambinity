using Ambinity.Views.Draw2DCanvas;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor.Canvas;

public partial class ProfileEditorCanvasView : UserControl
{
    private CanvasViewModelFactory _viewModelFactory;
    private FigureContextMenuProvider _contextMenuProvider;
    private ProfileEditorCanvasViewModel _viewModel;
    public ProfileEditorCanvasView()
    {
        InitializeComponent();
        _viewModelFactory = Ioc.Default.GetRequiredService<CanvasViewModelFactory>();
        _viewModel = _viewModelFactory.Get<ProfileEditorCanvasViewModel>();
        _viewModel.Canvas.FigureRightClicked += OnFigureRightClicked;
        _viewModel.Canvas.CanvasRightClicked += OnCanvasRightClicked;
        _contextMenuProvider = Ioc.Default.GetRequiredService<FigureContextMenuProvider>();
    }

    private void OnCanvasRightClicked(object? sender, CanvasClickEventArgs e)
    {
        if (_viewModel.IsLocked)
            return;
        _contextMenuProvider.CanvasVM = _viewModel;
        _contextMenuProvider.Init();
        var point = new Point(e.MousePosX, e.MousePosY);
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
