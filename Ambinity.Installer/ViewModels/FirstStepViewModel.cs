namespace Ambinity.Installer.ViewModels;

public class FirstStepViewModel : StepViewModelBase
{
    public FirstStepViewModel()
    {
        Header="Installation information";
        SubHeader="Ambinity requires internet connection to download the core services and content, click continue to get started";
        StepIndex = 1;
        InstallationDirectory = @"C:\Program Files\Ambinity";
        CanBack = true;
        CanCancel = false;
        CanForward = true;
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }
    public string InstallationDirectory { get; }
}