namespace Ambinity.Installer.ViewModels;
using Ambinity.Installer.Localization;

public class FirstStepUninstallViewModel : StepViewModelBase
{
    public FirstStepUninstallViewModel()
    {
        CanForward = true;
        CanCancel = true;
        CanBack = false;
        StepIndex = 0;
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
    }

    public string Header => Loc.Get("UnInstallAmbinity.Header.Content");
    public string SubHeader => Loc.Get("UnInstallAmbinity.SubHeader.Content");
}
