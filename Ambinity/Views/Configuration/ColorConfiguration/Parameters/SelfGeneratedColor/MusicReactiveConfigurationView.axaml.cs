using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public partial class MusicReactiveConfigurationView : UserControl
{
    public MusicReactiveConfigurationView()
    {
        InitializeComponent();
    }

    private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        infoTip.IsOpen = true;
    }

    private void NoSoundTeachingButton_OnClick(object? sender, RoutedEventArgs e)
    {
        nosoundTip.IsOpen = true;
    }
}