using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Screens.CaptureEngine;

public partial class ScreenPreview : UserControl
{
    public ScreenPreview()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as ScreenPreviewViewModel;
            vm.PreviewImageControl = DisplayPreviewImage;
    }
}