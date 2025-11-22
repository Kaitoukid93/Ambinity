using Ambinity.Views.Draw2DCanvas;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;

namespace Ambinity.Views.LayoutEditor.Canvas;

public partial class DeviceLayoutCanvasView : UserControl
{
    private CanvasViewModelFactory _viewModelFactory;
    private FigureContextMenuProvider _contextMenuProvider;
    private DeviceLayoutCanvasViewModel _viewModel;
    public DeviceLayoutCanvasView()
    {
        InitializeComponent();
        this.GotFocus += DeviceLayoutCanvasView_GotFocus;
    }

    private void DeviceLayoutCanvasView_GotFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
            _viewModel = DataContext as DeviceLayoutCanvasViewModel;
       // _viewModel?.Init();
    }


}
