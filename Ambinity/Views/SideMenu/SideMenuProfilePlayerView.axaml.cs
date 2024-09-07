using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuProfilePlayerView : UserControl
{
    public SideMenuProfilePlayerView()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetRequiredService<SideMenuProfilePlayerViewModel>();
        vm.ProfilePictureUpdated += OnProfilePictureUpdated;
    }

    private void OnProfilePictureUpdated()
    {
        profilePicture.InvalidateVisual();
    }
}