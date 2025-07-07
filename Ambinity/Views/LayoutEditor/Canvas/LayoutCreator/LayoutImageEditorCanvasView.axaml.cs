using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ambinity.Views.LayoutEditor.Canvas;

public partial class LayoutImageEditorCanvasView : UserControl
{
    public LayoutImageEditorCanvasView()
    {
        InitializeComponent();
        this.LayoutUpdated += OnLayoutUpdated;
    }
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {

        this.LayoutUpdated -= OnLayoutUpdated;
        var vm = this.DataContext as LayoutImageEditorCanvasViewModel;
        if (vm != null)
        {
            vm.FitCommand.Execute(null);
        }
    }
}
