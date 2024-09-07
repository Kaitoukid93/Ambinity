using System;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public abstract class WindowDialogViewModelBase : ViewModelBase
{
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;
    public ContentDialog Dialog => _dialog;

    public virtual void Init(ContentDialog dialog)
    {
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }

        _dialog = dialog;
        dialog.Closed += DialogOnClosed;
    }
    public virtual void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;

        DialogClosed?.Invoke(this, args);
    }
}