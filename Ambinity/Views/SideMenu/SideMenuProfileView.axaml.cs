using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuProfileView : UserControl
{
    private AppTourViewModel _appTourViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private readonly Window? _mainWindow;
    public SideMenuProfileView()
    {
        InitializeComponent();
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
    }
    private async  void OnAppTourStepChanged(ViewModelBase element)
    {
        if(element is not SideMenuProfileViewModel)
            return;
        if (element == this.DataContext as SideMenuProfileViewModel)
        {
            await ActivateGuide();
        }
    }

    //play button click
    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        //wait 1sec 
        await Task.Run(()=> Task.Delay(500));
        //next step
        _appTourViewModel.NextStep(Ioc.Default.GetRequiredService<SideMenuProfilePlayerViewModel>());
    }
    private async Task ActivateGuide()
    {
        //focus on this category
        
        var thisPlayButton = new AppTourElement(this.PlayButton, "Play Profile",
            "To Play or Pause this profile, click this button");
        var thisPlayButtonVm = AppTourElementHelper.GetAppTourElements(thisPlayButton,this._mainWindow);
        if(thisPlayButtonVm ==null)
            return;
        _appTourViewModel?.Show(thisPlayButtonVm,true,this._mainWindow);
        
    }
}