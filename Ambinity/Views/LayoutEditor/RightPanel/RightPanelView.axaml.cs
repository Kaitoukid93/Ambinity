using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.LayoutEditor;

public partial class RightPanelView : UserControl
{
    public RightPanelView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<RightPanelViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
    }

    private RightPanelViewModel _viewModel;

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