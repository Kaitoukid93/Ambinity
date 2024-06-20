using Ambinity.Views.Screens.Dashboard;
using Avalonia.Controls;
using Avalonia.Input;


namespace Ambinity.Views.Screens.Dashboard;

public partial class DashboardDeviceView : UserControl
{
    public DashboardDeviceView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var vm = this.DataContext as DashboardDeviceViewModel;
        if (vm == null || e.InitialPressMouseButton != MouseButton.Left)
            return;
        vm.SelectDevice();
        e.Handled = true;
    }
}