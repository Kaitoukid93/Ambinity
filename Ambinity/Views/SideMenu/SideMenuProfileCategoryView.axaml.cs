using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuProfileCategoryView : UserControl
{
    private SideMenuProfileCategoryViewModel vm;
    public SideMenuProfileCategoryView()
    {
        InitializeComponent();
        
    }
    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var vm = this.DataContext as SideMenuProfileCategoryViewModel;
        if (e.InitialPressMouseButton == MouseButton.Left)
            vm?.ToggleCollapsed.Execute(null);
    }
}