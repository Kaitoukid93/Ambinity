using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
#if NET8_0_MACOS
using AppKit;
#endif

namespace Ambinity.QuickAccess;

public partial class SystemTrayFlyoutWindowView : Window
{
    private SystemTrayFlyoutWindowViewModel _viewModel;
    public SystemTrayFlyoutWindowView()
    {
        InitializeComponent();

        CalculateFlyoutPosition();
        //Position = new PixelPoint(_screenWidth - (int)(Width + 20), _screenHeight - (int)(Height + 42));
        Topmost = true;
        this.Deactivated += OnWindowDeactivated;
        _viewModel = Ioc.Default.GetRequiredService<SystemTrayFlyoutWindowViewModel>();
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        this.Close();
        _viewModel?.Dispose();
    }
    private void CalculateFlyoutPosition()
    {
#if NET8_0_MACOS
        var screens = NSScreen.Screens;
        NSScreen currentScreen = NSScreen.MainScreen;
        var visibleFrame = currentScreen.VisibleFrame;
        var unsafeArea = (int)currentScreen.Frame.Height - (int)visibleFrame.Height;
        Position = new PixelPoint((int)visibleFrame.Right - (int)Width - 5, (int)visibleFrame.Top * -1 + unsafeArea + 5);
#else
        var _screenWidth = Screens.Primary.WorkingArea.Width;
        var _screenHeight = Screens.Primary.WorkingArea.Height;
        Position = new PixelPoint(_screenWidth - (int)(Width + 20), _screenHeight - (int)(Height + 42));
#endif

    }
}