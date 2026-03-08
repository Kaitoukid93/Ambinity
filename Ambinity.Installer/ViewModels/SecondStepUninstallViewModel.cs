using Ambinity.Installer.Services;
using Ambinity.Installer.Localization;
namespace Ambinity.Installer.ViewModels;

public class SecondStepUninstallViewModel : StepViewModelBase
{
    public SecondStepUninstallViewModel(InstallationService installationService)
    {
        _installationService = installationService;
        CanForward = true;
        CanBack = true;
        CanCancel = false;
        StepIndex = 1;
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
    }

    public string Header => Loc.Get("SecondStepUninstall.Header.Content");
    public string SubHeader => Loc.Get("SecondStepUninstall.SubHeader.Content");

    private bool _deleteAppConfig;
    private bool _deleteDevices;
    private bool _deleteData;
    private bool _deleteAll;
    private readonly InstallationService _installationService;

    public bool DeleteAppConfig
    {
        get => _deleteAppConfig;
        set
        {
            _deleteAppConfig = value;
            _installationService.RemoveAppConfig = value;
            OnPropertyChanged();
        }
    }

    public bool DeleteDevices
    {
        get => _deleteDevices;
        set
        {
            _deleteDevices = value;
            _installationService.RemoveDevices = value;
            OnPropertyChanged();
        }
    }

    public bool DeleteData
    {
        get => _deleteData;
        set
        {
            _deleteData = value;
            _installationService.RemoveData = value;
            OnPropertyChanged();
        }
    }

    public bool DeleteAll
    {
        get => _deleteAll;
        set
        {
            _deleteAll = value;
            DeleteDevices = value;
            DeleteAppConfig = value;
            DeleteData = value;
            _installationService.RemoveAppData = value;
            OnPropertyChanged();
        }
    }
}
