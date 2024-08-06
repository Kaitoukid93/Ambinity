using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class DialogService : IDialogService
{
    /// <summary>
    /// show input dialog for rename or add new stuff
    /// </summary>
    public async Task ShowInputDialog(InputDialogContentViewModel vm, string title, string primaryButtonText,
        string closeButtonText)
    {
        var dialog = new ContentDialog()
        {
            Title = title,
            PrimaryButtonText = primaryButtonText,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = closeButtonText
        };
        vm.Init(dialog);
        dialog.Content = new InputDialogContent()
        {
            DataContext = vm
        };

        var result = await dialog.ShowAsync();
    }

    public async Task ShowDownloadDialog(DownloadDialogViewModel vm, Window owner, bool showCancelButton)
    {
        var td = new TaskDialog
        {
            Title = vm.Title,
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = "Downloading",
            Content = vm.Description,
        };
        if (showCancelButton)
            td.Buttons = new List<TaskDialogButton>() { TaskDialogButton.CancelButton };
        vm.Init(td);
        td.XamlRoot = owner;
        var result = await td.ShowAsync();
    }
}