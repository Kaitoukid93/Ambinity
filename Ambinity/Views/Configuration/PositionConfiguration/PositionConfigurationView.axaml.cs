using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.Configuration.PositionConfiguration;

public partial class PositionConfigurationView : UserControl
{
    public PositionConfigurationView()
    {
        InitializeComponent();
        var nm = this.FindControl<NumberBox>("scaleNumberBox");
    }


    // private void InfoButton_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     infoTip.IsOpen = true;
    // }
}
