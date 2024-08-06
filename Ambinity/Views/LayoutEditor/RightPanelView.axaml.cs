using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.LayoutEditor;

public partial class RightPanelView : UserControl
{
    public RightPanelView()
    {
        InitializeComponent();
    }
    
    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        
    }
}