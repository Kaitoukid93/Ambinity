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
    private readonly AppTourElementProvider _appTourElementProvider;

    public SideMenuProfileCategoryView()
    {
        InitializeComponent();
        _appTourViewModel = Ioc.Default.GetRequiredService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
        _appTourElementProvider = Ioc.Default.GetRequiredService<AppTourElementProvider>();
    }


    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if (element is not SideMenuProfileCategoryViewModel)
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


    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var vm = this.DataContext as SideMenuProfileCategoryViewModel;
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            vm?.ToggleCollapsed.Execute(null);
            if (vm.IsExpanded)
                ActivateGuide();
        }
    }

    private async Task ActivateGuide()
    {
        //focus on this category

        var thisElement = new AppTourElement(this, "Profile Category",
            "Each category contains profiles that can be edited, click expand button to see all profiles");
        var thisElementVm = _appTourElementProvider.GetAppTourElements(thisElement);
        if (thisElementVm == null)
            return;
        _appTourViewModel?.Show(thisElementVm, true);

        //wait for user to expand the list
        var vm = this.DataContext as SideMenuProfileCategoryViewModel;
        while (!vm.IsExpanded)
        {
            await Task.Run(() => Task.Delay(100));
        }
        if(SidebarListBox.Items.Count ==0)
            return;
        var firstCategory = new AppTourElement(SidebarListBox.ContainerFromItem(SidebarListBox.Items.First()),
            "Profile",
            "To play this profile, click the play button. Click the button again to stop");
        var sidePanelVm = _appTourElementProvider.GetAppTourElements(firstCategory);
        if (sidePanelVm == null)
            return;
        _appTourViewModel?.Show(sidePanelVm, true);
        //next step
    }
}