using System;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class InputDialogContentViewModel : ViewModelBase
{
    private  ContentDialog _dialog;

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

        // do something with the result
        var resultHint = new ContentDialog()
        {
            Content = $"You chose \"{args.Result}\"",
            Title = "Result",
            PrimaryButtonText = "Thanks"
        };

        _ = resultHint.ShowAsync();
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
            if (RaiseAndSetIfChanged(ref _UserInput, value))
            {
                HandleUserInput();
            }
        }
    }

    private void HandleUserInput()
    {
        switch (UserInput.ToLowerInvariant())
        {
            case "accept":
            case "ok":
                _dialog.Hide(ContentDialogResult.Primary);
                break;

            case "dismiss":
            case "not ok":
                _dialog.Hide(ContentDialogResult.Secondary);
                break;

            case "cancel":
            case "close":
            case "hide":
                _dialog.Hide();
                break;
        }
    }

    private static readonly string[] _AvailableKeyWords = new[]
    {
        "Accept",
        "OK",
        "Dismiss",
        "Not OK",
        "Close",
        "Cancel",
        "Hide"
    };

    public string[] AvailableKeyWords => _AvailableKeyWords;
}