using AmbinityServer;

namespace Ambinity.Installer.ViewModels;

public class SecondStepViewModel : StepViewModelBase
{
    private AmbinityClient _client;
    public SecondStepViewModel(AmbinityClient client)
    {
        Header="Downloading content";
        SubHeader="Please dont close this window and make sure your internet connection is stable";
        StepIndex = 2;
        CanBack = false;
        CanCancel = false;
        CanForward = true;
        _client = client;
    }
    
    public string Header { get; set; }
    public string SubHeader { get; set; }
}