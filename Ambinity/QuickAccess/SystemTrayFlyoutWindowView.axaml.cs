using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.QuickAccess;

public partial class SystemTrayFlyoutWindowView : Window
{
    public SystemTrayFlyoutWindowView()
    {
        InitializeComponent();
        var _screenWidth = Screens.Primary.WorkingArea.Width;
        var _screenHeight = Screens.Primary.WorkingArea.Height;
        Position = new PixelPoint(_screenWidth - (int)(Width + 12), _screenHeight - (int)(Height + 12));
        Topmost = true;
        this.Deactivated += OnWindowDeactivated;
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        this.Close();
    }
    
}