using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public partial class LEDSplitToolConfigurationView : Window
{
    public LEDSplitToolConfigurationView()
    {
        InitializeComponent();
    }

    private void Cancel_Button_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    private void Accept_Button_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
