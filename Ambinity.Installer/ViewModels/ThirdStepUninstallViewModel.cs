using System;
using System.Threading.Tasks;
using Ambinity.Installer.Services;
using Ambinity.Installer.Localization;

namespace Ambinity.Installer.ViewModels;

public class ThirdStepUninstallViewModel : StepViewModelBase
{
    private readonly InstallationService _installationService;
    private IProgress<int> _downloadProgress;
    public ThirdStepUninstallViewModel( InstallationService installationService)
    {
        CanForward = false;
        CanBack = false;
        CanCancel = false;
        StepIndex = 2;
        IsBusy = true;
        UninstallationFinished = false;
        _installationService = installationService;
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
    private bool _uninstallationFinished;

    public bool UninstallationFinished
    {
        get => _uninstallationFinished;
        set
        {
            _uninstallationFinished = value;
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
    public async Task Init()
    {
        // Download the file
        IsBusy = true;
        OnPropertyChanged(nameof(IsBusy));
        UninstallationFinished = false;
        InstallationInformation = Loc.Get("ThirdStepUninstall.Uninstalling.Info");
        InstallationSubInformation = Loc.Get("ThirdStepUninstall.Closing.Info");
        await _installationService.RemoteShutdown();

        // Remove existing binaries
        InstallationSubInformation = Loc.Get("ThirdStepUninstall.Removing.Info");
        await _installationService.UninstallRelease(_downloadProgress, false);
        InstallationSubInformation = Loc.Get("ThirdStepUninstall.Cleaning.Info");
        _installationService.RemoveInstallKey();
        InstallationSubInformation = Loc.Get("ThirdStepUninstall.RemovingShortcuts.Info");
        _installationService.RemoveDesktopShortcut();
        InstallationInformation = Loc.Get("ThirdStepUninstall.Done.Info");
        InstallationSubInformation = Loc.Get("ThirdStepUninstall.Finished.Info");
        UninstallationFinished = true;
        IsBusy = false;
        OnPropertyChanged(nameof(IsBusy));
    }
    public string Header => UninstallationFinished ? Loc.Get("ThirdStepUninstall.Finished.Header") : Loc.Get("ThirdStepUninstall.Header.Content");
    public string SubHeader => UninstallationFinished ? Loc.Get("ThirdStepUninstall.Finished.SubHeader") : Loc.Get("ThirdStepUninstall.SubHeader.Content");
}
