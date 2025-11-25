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
    private readonly AppTourElementProvider _appTourElementProvider;
    private  SideMenuViewModel _vm;
    public SideMenuView()
    {
        InitializeComponent();
        //if show guide tour
        _appTourViewModel = Ioc.Default.GetRequiredService<AppTourViewModel>();
        _appTourElementProvider = Ioc.Default.GetRequiredService<AppTourElementProvider>();
        _appTourViewModel.NextStepActivated += OnApptourStepChanged;
        this.SizeChanged += OnLayoutUpdated;
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
    private async Task ActivateGuide()
    {
        //1- introduce side panel
        var sidePanel = new AppTourElement(this, "Side Menu",
            "Side menu provide quick access to available profiles, settings, and Now Playing profile");
        var sidePanelVm = _appTourElementProvider.GetAppTourElements(sidePanel);
        if (sidePanelVm == null)
            return;
        _appTourViewModel?.Show(sidePanelVm);
        await Task.Run(() => Task.Delay(1000));
        _appTourViewModel.NextStep(categories.Items.First() as SideMenuProfileCategoryViewModel);

    }
    private void OnLayoutUpdated(object? sender, System.EventArgs e)
    {
        _vm = this.DataContext as SideMenuViewModel;

        if (_vm == null)
            return;

        // measure available height
        double height = this.Bounds.Height;
        if (height < 700)
        {
            if (_vm.CurrentPlayingContent != _vm.MiniProfilePlayerViewModel)
                _vm.CurrentPlayingContent = _vm.MiniProfilePlayerViewModel;
        }
        else
        {
            if (_vm.CurrentPlayingContent != _vm.ProfilePlayerViewModel)
                _vm.CurrentPlayingContent = _vm.ProfilePlayerViewModel;
        }
    }
}

