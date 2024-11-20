using System;
using System.Threading.Tasks;
using Ambinity.Installer.Services;

namespace Ambinity.Installer.ViewModels;

public class ThirdStepUninstallViewModel : StepViewModelBase
{
    private readonly InstallationService _installationService;
    private IProgress<int> _downloadProgress;
    public ThirdStepUninstallViewModel( InstallationService installationService)
    {
        Header = "Uninstalling ambinity";
        SubHeader = "Please dont close this window";
        CanForward = false;
        CanBack = false;
        CanCancel = false;
        StepIndex = 2;
        IsBusy = true;
        _installationService = installationService;
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
    private bool _uninstallationFinished;

    public bool UninstallationFinished
    {
        get => _uninstallationFinished;
        set
        {
            _uninstallationFinished = value;
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
    public async Task Init()
    {
        // Download the file
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        UninstallationFinished = false;
        InstallationInformation = "Uninstalling";
        InstallationSubInformation = "Closing down Ambinity in case it's running...";
        await _installationService.RemoteShutdown();

        // Remove existing binaries
        InstallationSubInformation = "Removing old files...";
        await _installationService.UninstallRelease(_downloadProgress, false);
        InstallationSubInformation = "Cleaning up registry.";
        _installationService.RemoveInstallKey();
        InstallationSubInformation = "Removing shortcuts.";
        _installationService.RemoveDesktopShortcut();
        InstallationInformation = "Done";
        InstallationSubInformation = "Uninstallation finished!";
        UninstallationFinished = true;
        IsBusy = false;
        OnPropertyChanged(nameof(IsBusy));
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }
}