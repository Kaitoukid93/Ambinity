using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.QuickAccess;

public partial class SystemTrayFlyoutWindowView : Window
{
    private SystemTrayFlyoutWindowViewModel _viewModel;
    public SystemTrayFlyoutWindowView()
    {
        InitializeComponent();
        var _screenWidth = Screens.Primary.WorkingArea.Width;
        var _screenHeight = Screens.Primary.WorkingArea.Height;
        Position = new PixelPoint(_screenWidth - (int)(Width + 12), _screenHeight - (int)(Height + 12));
        Topmost = true;
        this.Deactivated += OnWindowDeactivated;
        _viewModel = Ioc.Default.GetRequiredService<SystemTrayFlyoutWindowViewModel>();
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
       this.Close();
       _viewModel?.Dispose();
    }
    
}