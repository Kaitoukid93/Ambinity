using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor;

public partial class LayerView : UserControl
{
    public LayerView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }


    private void ContainerGrid_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var border = sender as Grid;
        var dataContext = border.DataContext as LayerViewModel;
        if (dataContext != null)
        {
            dataContext.Figure.IsMouseOver = true;
        }
    }

    private void ContainerGrid_OnPointerExited(object? sender, PointerEventArgs e)
    {
        var border = sender as Grid;
        var dataContext = border.DataContext as LayerViewModel;
        if (dataContext != null)
        {
            dataContext.Figure.IsMouseOver = false;
        }
    }

    private void ContainerGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = sender as Grid;
        var isCtrlPressed = e.KeyModifiers == KeyModifiers.Control;
        var dataContext = border.DataContext as LayerViewModel;
        if (dataContext != null)
        {
            dataContext.OnLayerPointerPress(isCtrlPressed);
        }
    }

    private bool _isCtrlPressed;
    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
            _isCtrlPressed = true;
    }

    private void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
            _isCtrlPressed = false;
    }
}