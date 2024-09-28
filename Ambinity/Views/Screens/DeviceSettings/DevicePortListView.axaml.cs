using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Screens.DeviceSettings;

public partial class DevicePortListView : UserControl
{
    public DevicePortListView()
    {
        InitializeComponent();
    }

    private void ContainerGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var border = sender as Grid;
        var isCtrlPressed = e.KeyModifiers == KeyModifiers.Control;
        var dataContext = border.DataContext as DevicePortViewModel;
        if (dataContext != null)
        {
            dataContext.OnLayerPointerPress(isCtrlPressed);
        }
    }

    private void ContainerGrid_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var border = sender as Grid;
        var dataContext = border.DataContext as DevicePortViewModel;
        if (dataContext != null)
        {
            dataContext.IsMouseOver = true;
        }
    }

    private void ContainerGrid_OnPointerExited(object? sender, PointerEventArgs e)
    {
        var border = sender as Grid;
        var dataContext = border.DataContext as DevicePortViewModel;
        if (dataContext != null)
        {
            dataContext.IsMouseOver = false;
        }
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
       // throw new System.NotImplementedException();
    }
}