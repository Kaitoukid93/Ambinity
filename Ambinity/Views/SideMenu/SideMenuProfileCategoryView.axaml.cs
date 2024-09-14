using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.SideMenu;

public partial class SideMenuProfileCategoryView : UserControl
{
    private readonly AppTourViewModel _appTourViewModel;
    private readonly IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private readonly Window? _mainWindow;
    public SideMenuProfileCategoryView()
    {
        InitializeComponent();
        _appTourViewModel = Ioc.Default.GetRequiredService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
        _appTourElements = new List<AppTourElement>();
        var profileCategories = new  AppTourElement(this.profilesPanel, "Profiles",
            "Play any profile by click the play button. Profile name and icon can be customized. Click profile name to open profile editor");
        _appTourElements.Add(profileCategories);
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
    }
    

    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if(element is not SideMenuProfileCategoryViewModel)
            return;
        if (element == this.DataContext as SideMenuProfileCategoryViewModel)
        {
            await ActivateGuide();
        }
    }

    private async void OnListProfileExpanded(object? sender, EventArgs e)
    {
        await ActivateGuide();
    }


    private List<AppTourElement> _appTourElements;
    

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var vm = this.DataContext as SideMenuProfileCategoryViewModel;
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            vm?.ToggleCollapsed.Execute(null);
            if(vm.IsExpanded)
                ActivateGuide();
        }
            
    }
    private async Task ActivateGuide()
    {
        //focus on this category
        
        var thisElement = new AppTourElement(this, "Default Category",
            "Each category contains profile that can be add and delete, expand to see all profiles");
        var thisElementVm = AppTourElementHelper.GetAppTourElements(thisElement,this._mainWindow);
        if(thisElementVm ==null)
            return;
        _appTourViewModel?.Show(thisElementVm,true,this._mainWindow);
        
        //wait for user to expand the list
        var vm = this.DataContext as SideMenuProfileCategoryViewModel;
        while (!vm.IsExpanded)
        {
            await Task.Run(()=> Task.Delay(100));
        }
        var firstCategory = new AppTourElement(SidebarListBox.ContainerFromItem(SidebarListBox.Items.First()), "Default Profile",
            "To play this profile, click the play button. Click the button again to stop");
        var sidePanelVm = AppTourElementHelper.GetAppTourElements(firstCategory,this._mainWindow);
        if(sidePanelVm ==null)
            return;
        _appTourViewModel?.Show(sidePanelVm,true,this._mainWindow);
        //next step
       
    }
    
}