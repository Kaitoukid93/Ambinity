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

public partial class SideMenuProfilePlayerView : UserControl
{
    private AppTourViewModel _appTourViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private readonly Window? _mainWindow;
    private int _playClickCount;
    private int _profileClickCount;

    public SideMenuProfilePlayerView()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetRequiredService<SideMenuProfilePlayerViewModel>();
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        vm.ProfilePictureUpdated += OnProfilePictureUpdated;
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
    }

    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if (element is not SideMenuProfilePlayerViewModel)
            return;
        if (element == this.DataContext as SideMenuProfilePlayerViewModel)
        {
            await ActivateGuide();
        }
    }

    private void OnProfilePictureUpdated()
    {
        profilePicture.InvalidateVisual();
    }

    private async Task ActivateGuide()
    {
        //focus on this category

        var thisProfileButton = new AppTourElement(this.ProfileButton, "Current Playing Profile",
            "To Play or Pause this profile, click Play button. To edit this access Now Playing Profile anywhere, click this image");
        var thisProfileButtonVm = AppTourElementHelper.GetAppTourElements(thisProfileButton, this._mainWindow);
        if (thisProfileButtonVm == null)
            return;
        _appTourViewModel?.Show(thisProfileButtonVm, true, this._mainWindow);
        //wait for user to interact
        while (!PlayButton.IsVisible)
        {
            await Task.Delay(100);
        }

        //show play button guide
        var thisPlayButton = new AppTourElement(this.PlayButton, "Current Playing Profile",
            "To Play or Pause this profile, click Play button. To edit this access Now Playing Profile anywhere, click this image");
        var thisPlayButtonVm = AppTourElementHelper.GetAppTourElements(thisPlayButton, this._mainWindow);
        if (thisPlayButtonVm == null)
            return;
        _appTourViewModel?.Show(thisPlayButtonVm, true, this._mainWindow);
        while (_playClickCount == 0)
        {
            await Task.Delay(100);
        }

        _appTourViewModel?.Show(thisProfileButtonVm, true, this._mainWindow);
        while (_profileClickCount == 0)
        {
            await Task.Delay(3000);
        }

        _appTourViewModel?.NextStep(Ioc.Default.GetRequiredService<ToolsViewModel>());
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