using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor;

public partial class DeviceLayoutRightPanelView : UserControl
{
    public DeviceLayoutRightPanelView()
    {
        InitializeComponent();
    }

    private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        infotip.IsOpen = true;
    }
}