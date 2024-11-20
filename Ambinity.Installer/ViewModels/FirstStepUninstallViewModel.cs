namespace Ambinity.Installer.ViewModels;

public class FirstStepUninstallViewModel : StepViewModelBase
{
    public FirstStepUninstallViewModel()
    {
        Header = "Uninstall Ambinity";
        SubHeader = "Following these steps will remove Ambinity from your computer";
        CanForward = true;
        CanCancel = true;
        CanBack = false;
        StepIndex = 0;
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }
}