using System;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using AmbinityCore.Models.Ultilities;
using AmbinityServer.Download;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class DownloadDialogViewModel : ViewModelBase
{
    private TaskDialog _taskDialog;
    private Func<Task> _downloader;
    public event Action DialogOpened;
    public IProgress<ProgressInformation> ProgressInformation { get; set; }

    public DownloadDialogViewModel( string title)
    {
        Title = title;
    }

    public void Init(TaskDialog taskDialog)
    {
       
        _taskDialog = taskDialog;
        _taskDialog.Buttons.First().IsEnabled = false;
        _taskDialog.Opened += async (s, e) => { DialogOpened?.Invoke(); };
        ProgressInformation = new Progress<ProgressInformation>((s) =>
        {
            Progress = s.Progress;
            Description = s.Info;
        });
    }

    /// <summary>
    /// Title of the dialog
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description of the dialog
    /// </summary>
    private string _description;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

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
                _taskDialog.Buttons.First().IsEnabled=true;
        }
    }
}