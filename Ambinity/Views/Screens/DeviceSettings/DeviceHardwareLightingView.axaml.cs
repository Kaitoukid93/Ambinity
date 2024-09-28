using System.ComponentModel;
using Ambinity.Views.LayoutEditor;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.DeviceSettings;

public partial class DeviceHardwareLightingView : UserControl
{
    private DeviceHardwareLightingViewModel _viewModel;
    public DeviceHardwareLightingView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<DeviceHardwareLightingViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
    }
    private void CloseFlyout()
    {
        FlyoutBase.GetAttachedFlyout(this.colorPaletteButton).Hide();
    }

    private void OpenFlyout()
    {
        FlyoutBase.ShowAttachedFlyout(this.colorPaletteButton);
    }

    private void PopupFlyoutBase_OnClosing(object? sender, CancelEventArgs e)
    {
        _viewModel.OnFlyoutClosing();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.GetAttachedFlyout(this.colorPaletteButton).Hide();
    }
}