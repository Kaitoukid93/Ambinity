using System;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class InputDialogContentViewModel : ViewModelBase
{
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;
    public InputDialogContentViewModel()
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

    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;

        DialogClosed?.Invoke(this,args);
    }

    private string _UserInput;

    /// <summary>
    /// Gets or sets the user input to check 
    /// </summary>
    public string UserInput
    {
        get => _UserInput;
        set
        {
            SetProperty(ref _UserInput, value);
        }
    }
    
    private static readonly string[] _AvailableKeyWords = new[]
    {
        "Static",
        "Rainbow",
        "Party",
        "Music",
        "Favorite",
    };

    public string[] AvailableKeyWords => _AvailableKeyWords;
}