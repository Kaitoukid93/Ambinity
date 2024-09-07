using System;
using System.Collections.Generic;
using Ambinity.ViewModels;
using AmbinityCore.Colors;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer.Download;

namespace Ambinity.Views.SplashScreen;

public class SplashViewModel : ViewModelBase
{
    public SplashViewModel()
    {
        DownloadProgress = new Progress<DownloadProgress>((p) =>
        {
            Progress = p.Progress;
            Status = p.Status;
        });
    }
    public IProgress<DownloadProgress> DownloadProgress { get; set; }
    private int _progress;

    public int Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }
    private void OnRepositoryLoaded(string obj)
    {
        Status = obj + " loaded";
    }

    private List<CollectableItemRepository> _repositories;
    private string _status;

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }
}