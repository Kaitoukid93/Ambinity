using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Configuration.PositionConfiguration;

public partial class PositionConfigurationView : UserControl
{
    public PositionConfigurationView()
    {
        InitializeComponent();
    }

    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as PositionConfigurationViewModel;
        if(vm == null)
            return;
        vm.TryUpdateItemProperty();
    }

    private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        infoTip.IsOpen = true;
    }
}