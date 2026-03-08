using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Installer.Localization;
using System.IO;

namespace Ambinity.Installer.ViewModels;

public class FirstStepModifyViewModel : StepViewModelBase
{
    public FirstStepModifyViewModel()
    {
        CanForward = true;
        CanCancel = true;
        StepIndex = 0;
        SelectedOption = "update";
        ModifyOptionCommand = new RelayCommand<string>(ModifyOptionChanged);
        Loc.LanguageChanged += OnLanguageChanged;

    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
    }

    public string Header => Loc.Get("ModifyAmbinity.Header.Content");
    public string SubHeader => Loc.Get("ModifyAmbinity.SubHeader.Content");

    private void ModifyOptionChanged(string? option)
    {
        SelectedOption = option;
    }

    public ICommand ModifyOptionCommand { get; }
    public string SelectedOption { get; set; }
}
