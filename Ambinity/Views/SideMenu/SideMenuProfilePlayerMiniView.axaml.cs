using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Screens.ProfileEditor;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuProfilePlayerMiniView : UserControl
{
    private int _playClickCount;
    private int _profileClickCount;

    public SideMenuProfilePlayerMiniView()
    {
        InitializeComponent();
    }

    private void PlayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _playClickCount++;
    }

    private void ProfileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source == ProfileButton)
            _profileClickCount++;
    }
}
