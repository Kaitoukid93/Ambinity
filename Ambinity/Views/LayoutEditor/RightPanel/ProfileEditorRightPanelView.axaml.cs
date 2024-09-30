using System.ComponentModel;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.AppTour;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.LayoutEditor;

public partial class ProfileEditorRightPanelView : UserControl
{
    private AppTourViewModel _appTourViewModel;
    private AppTourElementProvider _appTourElementProvider;

    public ProfileEditorRightPanelView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<ProfileEditorRightPanelViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
        _appTourElementProvider = Ioc.Default.GetRequiredService<AppTourElementProvider>();
    }

    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if (element is ProfileEditorRightPanelViewModel)
        {
            await ActivateGuide();
        }
    }

    private ProfileEditorRightPanelViewModel _viewModel;

    private void CloseFlyout()
    {
        FlyoutBase.GetAttachedFlyout(this).Hide();
    }

    private void OpenFlyout()
    {
        FlyoutBase.ShowAttachedFlyout(this);
    }


    private void PopupFlyoutBase_OnClosing(object? sender, CancelEventArgs e)
    {
        _viewModel.OnFlyoutClosing();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.GetAttachedFlyout(this).Hide();
    }

    private async Task ActivateGuide()
    {
        //focus on this category
        var thisRightPanel = new AppTourElement(this, "Properties Panel",
            "This Properties Panel allow you to edit selected zone's properties such as Colors, Position, Behavior...");
        var thisProfileButtonVm = _appTourElementProvider.GetAppTourElements(thisRightPanel, isReverse: true);
        if (thisProfileButtonVm == null)
            return;
        _appTourViewModel?.Show(thisProfileButtonVm, true);
        for (int i = 0; i <= 5; i++)
        {
            await Task.Delay(1000);
            thisProfileButtonVm.SkipButtonContent = "Close" + "(" + (5 - i).ToString() + ")";
        }

        _appTourViewModel.SkipCommand.Execute(null);
        //enable interact

        //_appTourViewModel?.NextStep(Ioc.Default.GetRequiredService<ToolsViewModel>());
    }
}