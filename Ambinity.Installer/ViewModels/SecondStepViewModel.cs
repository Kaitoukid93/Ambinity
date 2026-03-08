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
using Ambinity.Installer.Localization;

namespace Ambinity.Installer.ViewModels;

public class SecondStepViewModel : StepViewModelBase
{
    private IProgress<int> _downloadProgress;
    private InstallationService _installationService;
    public event Action<Color> AccentColorChanged;

    public SecondStepViewModel(InstallationService installationService)
    {
        _installationService = installationService;
        StepIndex = 2;
        CanBack = false;
        CanCancel = false;
        CanForward = true;
        InstallationFinished = false;
        _downloadProgress = new Progress<int>((p) => { CurrentProgress = p; });
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
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
        InstallationInformation = Loc.Get("SecondStep.Downloading.Content");
        InstallationSubInformation = Loc.Get("SecondStep.Downloading.SubContent");
        var (file, releaseInfo) = await _installationService.DownloadRelease(_downloadProgress,info);

        InstallationSubInformation = Loc.Get("SecondStep.Closing.Content");
        await _installationService.RemoteShutdown();

        // Remove existing binaries
        InstallationSubInformation = Loc.Get("SecondStep.Removing.Content");
        await _installationService.UninstallRelease(_downloadProgress, true);

        // Extract the ZIP
        InstallationInformation = Loc.Get("SecondStep.Extracting.Content");
        InstallationSubInformation = Loc.Get("SecondStep.Extracting.SubContent") + releaseInfo.Version;
        await _installationService.InstallRelease(file, _downloadProgress);
        // Change to default accent color
        var _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = Avalonia.Media.Color.Parse("#FF1DB954");
        AccentColorChanged?.Invoke(Avalonia.Media.Color.Parse("#FF1DB954"));
        // Create registry keys
        InstallationSubInformation = Loc.Get("SecondStep.Finalizing.Content");
        _installationService.CreateInstallKey();

        // Remove the installer archive
        File.Delete(file);
        IsDownloading = false;
        InstallationInformation = Loc.Get("SecondStep.Done.Content");
        InstallationSubInformation = Loc.Get("SecondStep.Finished.Content");
        InstallationFinished = true;
        IsBusy = false;
        OnPropertyChanged(nameof(IsBusy));
    }


    public string Header => InstallationFinished ? Loc.Get("SecondStep.Finished.Content") : Loc.Get("SecondStep.Header.Content");
    public string SubHeader => InstallationFinished ? Loc.Get("SecondStep.Finished.SubContent") : Loc.Get("SecondStep.SubHeader.Content");
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
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(SubHeader));
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
