using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor.Canvas;

public partial class LEDLayoutCreatorCanvasView : UserControl
{
    public LEDLayoutCreatorCanvasView()
    {
        InitializeComponent();
        this.LayoutUpdated += OnLayoutUpdated;
    }
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {

        this.LayoutUpdated -= OnLayoutUpdated;
        var vm = this.DataContext as LEDLayoutCreatorCanvasViewModel;
        if (vm != null)
        {
            vm.FitCommand.Execute(null);
        }
    }
}
