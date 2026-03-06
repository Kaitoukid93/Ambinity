using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Installer.Localization;

namespace Ambinity.Installer.ViewModels;

public class FirstStepModifyViewModel : StepViewModelBase
{
    public FirstStepModifyViewModel()
    {
        CanForward = true;
        CanCancel = true;
        Header  = Loc.Get("ModifyAmbinity.Header.Content");
        SubHeader = "Ambinity is installed, please choose one of the options below";
        StepIndex = 0;
        SelectedOption = "update";
        ModifyOptionCommand = new RelayCommand<string>(ModifyOptionChanged);
    }

    private void ModifyOptionChanged(string? option)
    {
        SelectedOption = option;
    }

    public string Header { get; set; }
    public string SubHeader { get; set; }
    public ICommand ModifyOptionCommand { get; }
    public string SelectedOption { get; set; }
}
