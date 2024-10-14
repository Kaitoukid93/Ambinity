using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.QuickAccess.Controls;

public partial class CustomToggleSplitButton : UserControl
{
    public CustomToggleSplitButton()
    {
        InitializeComponent();
    }

    private void ButtonMore_OnClick(object? sender, RoutedEventArgs e)
    {
        ToggleButton buttonMore = (ToggleButton)e.Source!;
        buttonMore.IsChecked = true;
    }
}