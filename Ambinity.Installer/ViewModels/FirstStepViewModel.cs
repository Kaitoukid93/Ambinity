namespace Ambinity.Installer.ViewModels;
using Ambinity.Installer.Localization;
public class FirstStepViewModel : StepViewModelBase
{
    public FirstStepViewModel()
    {
        StepIndex = 1;
        InstallationDirectory = @"C:\Program Files\Ambinity";
        CanBack = true;
        CanCancel = false;
        CanForward = true;
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
    }

    public string Header => Loc.Get("FirstStep.Header.Content");
    public string SubHeader => Loc.Get("FirstStep.SubHeader.Content");
    public string InstallationDirectory { get; }
}
