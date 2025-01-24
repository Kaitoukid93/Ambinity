using System;
using Ambinity.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor.LEDLayoutEditor;

public partial class ColorTestingView : Window
{
    public ColorTestingView()
    {
        InitializeComponent();
        var vm = this.DataContext as ColorTestingViewModel;
    }
}