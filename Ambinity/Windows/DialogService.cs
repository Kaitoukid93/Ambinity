using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
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
}