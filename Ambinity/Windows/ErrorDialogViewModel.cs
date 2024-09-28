using System;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class ErrorDialogViewModel : ViewModelBase
{
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;

    public ErrorDialogViewModel()
    {
    }

    public void Init(ContentDialog dialog)
    {
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }

        _dialog = dialog;
        dialog.Closed += DialogOnClosed;
    }
    public string Content { get; set; }
    public string ErrorMessage { get; set; }

    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;

        DialogClosed?.Invoke(this, args);
    }
}