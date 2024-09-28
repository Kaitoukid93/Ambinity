using System;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class LoadingDialogViewModel : ViewModelBase
{
    private TaskDialog _taskDialog;
    private Func<Task> _downloader;
    public event Action DialogOpened;
    private string _errorMessage;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    private string _successMessage;

    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            _successMessage = value;
            OnPropertyChanged();
        }
    }
    public LoadingDialogViewModel()
    {
        
    }

    public void Init(TaskDialog taskDialog)
    {
        _taskDialog = taskDialog;
        _taskDialog.Buttons.First().IsEnabled = false;
        _taskDialog.Opened += async (s, e) => { DialogOpened?.Invoke(); };
    }

    /// <summary>
    /// Title of the dialog
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description of the dialog
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Icon to display on the left
    /// </summary>
    public string Icon { get; set; }

    private bool _errorMessageVissible = false;
    public bool ErrorMessageVissible
    {
        get => _errorMessageVissible;
        set
        {
            _errorMessageVissible = value;
            OnPropertyChanged();
        }
    }
    private bool _successMessageVissible = false;
    public bool SuccessMessageVissible
    {
        get => _successMessageVissible;
        set
        {
            _successMessageVissible = value;
            OnPropertyChanged();
        }
    }
    private bool _progressbarVissible = true;
    public bool ProgressbarVissible
    {
        get => _progressbarVissible;
        set
        {
            _progressbarVissible = value;
            OnPropertyChanged();
        }
    }
    public void Close()
    {
        _taskDialog.Hide();
    }

    public void ShowError(string errorMessage)
    {
        ProgressbarVissible = false;
        ErrorMessageVissible = true;
        ErrorMessage = errorMessage;
        _taskDialog.Header = "Error";
        _taskDialog.Buttons.First().Text = "OK";
        _taskDialog.Buttons.First().IsEnabled = true;
    }
    public void ShowSuccess(string successMessage)
    {
        ProgressbarVissible = false;
        ErrorMessageVissible = false;
        SuccessMessageVissible = true;
        SuccessMessage = successMessage;
        _taskDialog.Header = "Done";
        _taskDialog.Buttons.First().Text = "Close";
        _taskDialog.Buttons.First().IsEnabled = true;
    }
}