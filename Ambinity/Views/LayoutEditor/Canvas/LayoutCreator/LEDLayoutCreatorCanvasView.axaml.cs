using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    private void OnGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Oem6) // This is the key code for ']'
        {
            var vm = this.DataContext as LEDLayoutCreatorCanvasViewModel;
            if (vm != null)
            {
                vm.IncreaseCommand.Execute(null);
            }
        }
        if (e.Key == Key.Oem4) // This is the key code for ']'
        {
            var vm = this.DataContext as LEDLayoutCreatorCanvasViewModel;
            if (vm != null)
            {
                vm.DecreaseCommand.Execute(null);
            }
        }
         if (e.Key == Key.Delete) // This is the key code for ']'
        {
            var vm = this.DataContext as LEDLayoutCreatorCanvasViewModel;
            if (vm != null)
            {
                vm.Canvas.RemoveSelected();
            }
        }
    }
}
