using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Screens.DeviceSettings;

public partial class PortDetailView : UserControl
{
    private readonly PortDetailViewModel _viewModel;
    private bool shouldShowTip;
    public PortDetailView()
    {
        InitializeComponent();
        _viewModel = Ioc.Default.GetRequiredService<PortDetailViewModel>();
        _viewModel.OpenFlyoutEvent += OpenFlyout;
        _viewModel.CloseFlyoutEvent += CloseFlyout;
        shouldShowTip = true;
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

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (shouldShowTip)
        {
           // libraryTip.IsOpen = true;
            shouldShowTip = false;
        }
        
    }

    private void ShowLibraryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //libraryTip.IsOpen = false;
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
       // libraryTip.IsOpen = false;
    }
}