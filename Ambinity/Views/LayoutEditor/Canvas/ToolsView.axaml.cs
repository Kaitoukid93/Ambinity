using System.ComponentModel;
using System.Linq;
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

namespace Ambinity.Views.LayoutEditor;
public partial class ToolsView : UserControl
{
    
    private AppTourViewModel _appTourViewModel;
    private AppTourElementProvider _appTourElementProvider;
    private int flyoutButtonClickCount;
    private ToolsViewModel _viewModel;
    public ToolsView()
    {
        _appTourViewModel = Ioc.Default.GetService<AppTourViewModel>();
        _appTourElementProvider = Ioc.Default.GetService<AppTourElementProvider>();
        _appTourViewModel.NextStepActivated += OnAppTourStepChanged;
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<ToolsViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
    }

    private async void OnAppTourStepChanged(ViewModelBase element)
    {
        if (element is ToolsViewModel)
        {
            await ActivateGuide();
        }
    }
    private async Task ActivateGuide()
    {
        //focus on this category
        while (!tools.IsLoaded)
        {
            await Task.Delay(100);
        }
        var firstTool = new AppTourElement(tools.ContainerFromItem(tools.Items.First()), "Add Colors Zone",
            "Click to add new color zone, there are three zone shapes available : Rectangle, Ellipse and Polyline. Zone can only be added when profile is not playing");
        var firstToolVm = _appTourElementProvider.GetAppTourElements(firstTool,true);
        if(firstToolVm ==null)
            return;
        _appTourViewModel?.Show(firstToolVm,true);
        //wait for user to interact
        while (flyoutButtonClickCount==0)
        {
            await Task.Delay(100);
        }
        _appTourViewModel.NextStep(Ioc.Default.GetRequiredService<ProfileEditorRightPanelViewModel>());
    }

    private void MenuFlyoutItem_OnClick(object? sender, RoutedEventArgs e)
    {
        flyoutButtonClickCount++;
    }
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
}