using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Installer.Services;
using AmbinityServer;
using AmbinityServer.AppRelease;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using FluentAvalonia.Styling;
using Serilog;

namespace Ambinity.Installer.ViewModels;

public class SecondStepViewModel : StepViewModelBase
{
    private IProgress<int> _downloadProgress;
    private InstallationService _installationService;
    public event Action<Color> AccentColorChanged;
   
    public SecondStepViewModel(InstallationService installationService)
    {
        _installationService = installationService;
        Header = "Installing Ambinity";
        SubHeader = "Please dont close this window and make sure your internet connection is stable";
        StepIndex = 2;
        CanBack = false;
        CanCancel = false;
        CanForward = true;
        _downloadProgress = new Progress<int>((p) => { CurrentProgress = p; });
    }

    private int _currentProgress;

    public int CurrentProgress
    {
        get => _currentProgress;
        set
        {
            _currentProgress = value;
            OnPropertyChanged();
        }
    }

    public async Task Init(AppReleaseInformation info=null)
    {
        // Download the file
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        InstallationFinished = false;
        IsDownloading = true;
        InstallationInformation = "Downloading";
        InstallationSubInformation = "Downloading latest Ambinity release...";
        var (file, releaseInfo) = await _installationService.DownloadRelease(_downloadProgress,info);

        InstallationSubInformation = "Closing down Ambinity in case it's running...";
        await _installationService.RemoteShutdown();

        // Remove existing binaries
        InstallationSubInformation = "Removing old files...";
        await _installationService.UninstallRelease(_downloadProgress, true);

        // Extract the ZIP
        InstallationInformation = "Extracting";
        InstallationSubInformation = "Extracting Ambinity " + releaseInfo.Version;
        await _installationService.InstallRelease(file, _downloadProgress);
        // Change to default accent color
        var _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = Avalonia.Media.Color.Parse("#FF1DB954");
        AccentColorChanged?.Invoke(Avalonia.Media.Color.Parse("#FF1DB954"));
        // Create registry keys
        InstallationSubInformation = "Finalizing installation...";
        _installationService.CreateInstallKey();

        // Remove the installer archive
        File.Delete(file);
        IsDownloading = false;
        InstallationInformation = "Done";
        InstallationSubInformation = "Installation finished!";
        InstallationFinished = true;
        IsBusy = false;
        OnPropertyChanged(nameof(IsBusy));
    }


    public string Header { get; set; }
    public string SubHeader { get; set; }
    private bool _isDownloading;

    public bool IsDownloading
    {
        get => _isDownloading;
        set
        {
            _isDownloading = value;
            OnPropertyChanged();
        }
    }
    private bool _installationFinished;

    public bool InstallationFinished
    {
        get => _installationFinished;
        set
        {
            _installationFinished = value;
            OnPropertyChanged();
        }
    }
    private string _installationInformation;

    public string InstallationInformation
    {
        get => _installationInformation;
        set
        {
            _installationInformation = value;
            OnPropertyChanged();
        }
    }
    private string _installationSubInformation;
    public string InstallationSubInformation
    {
        get => _installationSubInformation;
        set
        {
            _installationSubInformation = value;
            OnPropertyChanged();
        }
    }

}