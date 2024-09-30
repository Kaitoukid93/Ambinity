using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.DeviceSettings;

public partial class PortDetailView : UserControl
{
    private readonly PortDetailViewModel _viewModel;

    public PortDetailView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<PortDetailViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
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