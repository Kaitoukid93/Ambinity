using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuView : UserControl
{
    private readonly AppTourViewModel _appTourViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private readonly Window? _mainWindow;
    public SideMenuView()
    {
        InitializeComponent();
        //if show guide tour
        _appTourViewModel = Ioc.Default.GetRequiredService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnApptourStepChanged;
        _appTourElements = new List<AppTourElement>();
        var sidePanel = new AppTourElement(this, "Side Menu",
            "Side menu provide quick access to available profiles, settings, and Now Playing profile");
        _appTourElements.Add(sidePanel);
        var profileCategories = new  AppTourElement(this.categories, "Profile Categories",
            "Profile Categories contains default profiles and downloaded profiles, you can collapse and expand just like window explorer");
        _appTourElements.Add(profileCategories);
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        
    }

    private async void OnApptourStepChanged(ViewModelBase element)
    {
        if (element is SideMenuViewModel)
        {
            while (!categories.IsLoaded)
            {
                await Task.Delay(100);
            }
            await ActivateGuide();
        }
      
    }

    private List<AppTourElement> _appTourElements;


    private async Task ActivateGuide()
    {
        //1- introduce side panel
       
        var sidePanelVm = AppTourElementHelper.GetAppTourElements(_appTourElements[0],this._mainWindow);
        if(sidePanelVm ==null)
            return;
        _appTourViewModel?.Show(sidePanelVm,true,this._mainWindow);

        await Task.Run(() => Task.Delay(1000));
        
        //activate profile guide
        
        _appTourViewModel.NextStep(categories.Items.First() as SideMenuProfileCategoryViewModel);
        //2- introduce categories section
        //3- introduce first profile section
        //4- introduce profile section function ( buttons, click to open, popup menu??)
        //
    }
}