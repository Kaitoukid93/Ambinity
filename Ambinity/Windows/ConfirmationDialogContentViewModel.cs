using System;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class ConfirmationDialogContentViewModel : ViewModelBase
{
    
    
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;

    public ConfirmationDialogContentViewModel()
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

    private string _content;
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }
    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;

        DialogClosed?.Invoke(this, args);
    }
}