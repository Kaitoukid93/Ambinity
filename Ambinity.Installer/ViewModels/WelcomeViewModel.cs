namespace Ambinity.Installer.ViewModels;

public class WelcomeViewModel : StepViewModelBase
{
    public WelcomeViewModel()
    {
        Header="Welcome to Ambinity";
        SubHeader="Please select your preferred language";
        StepIndex = 0;
        CanCancel = true;
        CanForward = true;
        CanBack = false;
    }
    public string Header { get; set; }
    public string SubHeader { get; set; }
}
