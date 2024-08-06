using System;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class DownloadDialogViewModel : ViewModelBase
{
    private TaskDialog _taskDialog;
    private Func<Task> _downloader;
    public event Action DialogOpened;

    public DownloadDialogViewModel()
    {
    }

    public void Init(TaskDialog taskDialog)
    {
        _taskDialog = taskDialog;
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

    private int _progress;

    public int Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
            _taskDialog.SetProgressBarState(value, TaskDialogProgressState.Normal);
            if (value == 100)
                _taskDialog.Hide();
        }
    }
}