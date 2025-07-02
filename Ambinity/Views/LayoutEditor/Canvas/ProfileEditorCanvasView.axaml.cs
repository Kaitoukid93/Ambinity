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
    }


}
