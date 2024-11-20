using Ambinity.Installer.Services;

namespace Ambinity.Installer.ViewModels;

public class SecondStepUninstallViewModel : StepViewModelBase
{
    public SecondStepUninstallViewModel(InstallationService installationService)
    {
        _installationService = installationService;
        Header = "Uninstall options";
        SubHeader = "Please chose what you want to remove";
        CanForward = true;
        CanBack = true;
        CanCancel = false;
        StepIndex = 1;
    }

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

    public string Header { get; set; }
    public string SubHeader { get; set; }
}