using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;

namespace Ambinity.Views.Debug;

public partial class DebugWindowView : Window
{
    public DebugWindowView()
    {
        InitializeComponent();
        this.Closed += OnDebugWindowClosed;
    }

    private void OnDebugWindowClosed(object? sender, EventArgs e)
    {
        if (DataContext is DebugWindowViewModel vm)
            vm.CloseWindow();
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!(LogsScrollViewer.Extent.Height - LogsScrollViewer.Offset.Y - LogsScrollViewer.Bounds.Bottom <= 60))
            return;
        Dispatcher.UIThread.Post(() => LogsScrollViewer.ScrollToEnd(), DispatcherPriority.Normal);
    }
}
