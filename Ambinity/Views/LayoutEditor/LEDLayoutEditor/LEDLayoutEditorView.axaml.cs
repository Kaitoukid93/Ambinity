using System;
using System.IO;
using AmbinityCore.Models.Device.LED;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Path = Avalonia.Controls.Shapes.Path;

namespace Ambinity.Views.LayoutEditor.LEDLayoutEditor;

public partial class LEDLayoutEditorView : Window
{
    private LEDLayoutEditorViewModel _viewModel;
    public LEDLayoutEditorView()
    {
        InitializeComponent();
        this.Closed += OnUserClosingWindow;
    }

    private void OnUserClosingWindow(object? sender, EventArgs e)
    {
        var vm = this.DataContext as LEDLayoutEditorViewModel;
        if (vm == null)
            return;
        if (vm.IsInIndexSetupMode)
            vm.CancelIndexSetup();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _viewModel = this.DataContext as LEDLayoutEditorViewModel;
        var led = (sender as PathIcon).DataContext as AmbinityLEDViewModel;
        if (led != null)
        {
            if (_viewModel.IsInIndexSetupMode)
            {
                _viewModel.SetIndex(led);
            }
            else
                _viewModel.ToggleLED(led);
        }
    }


}
