using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ambinity.Installer.Services;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using Ambinity.Installer.Localization;

namespace Ambinity.Installer.ViewModels;

public class UninstallViewModel : ViewModelBase
{
    public UninstallViewModel(FirstStepUninstallViewModel firstStepViewModel,SecondStepUninstallViewModel secondStepViewModel,ThirdStepUninstallViewModel thirdStepUninstallViewModel,
        InstallationService installationService)
    {
        _installationService = installationService;
        NextCommand = new RelayCommand(NexStep);
        BackCommand = new RelayCommand(PreviousStep);
        FinishCommand = new RelayCommand(FinishSetup);
        CancelCommand = new RelayCommand(CancelUninstall);
        //init steps based on parameter
        Steps =
        [
            firstStepViewModel,
            secondStepViewModel,
            thirdStepUninstallViewModel
        ];
    }

    private void CancelUninstall()
    {
        CloseWindowRequested?.Invoke();
    }

    public event Action CloseWindowRequested;
    public void Init()
    {
        AmbinityImage = new AmbinityImageViewModel(new SolidColorBrush(Colors.LightGray));
        CurrentView = Steps.First();
    }

    private void NexStep()
    {
        if (CurrentView.StepIndex == Steps.Count - 1)
            return;
        CurrentView = Steps[CurrentView.StepIndex + 1];
        if (CurrentView is ThirdStepUninstallViewModel thirdStepViewModel)
        {
            thirdStepViewModel.Init();
        }
    }

    private void PreviousStep()
    {
        if (CurrentView.StepIndex != 1)
            return;
        CurrentView = Steps[0];
    }
    private void FinishSetup()
    {

        CloseWindowRequested?.Invoke();
    }
    public string Header { get; set; }
    public string NextButtonContent => Loc.Get("Uninstall.NextButton.Content");
    private StepViewModelBase _currentView;
    private readonly InstallationService _installationService;

    public StepViewModelBase CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }
    public ICommand NextCommand { get; set; }
    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
    public List<StepViewModelBase> Steps { get; set; }
    private AmbinityImageViewModel _ambinityImage;

    public AmbinityImageViewModel AmbinityImage
    {
        get => _ambinityImage;
        set
        {
            _ambinityImage = value;
            OnPropertyChanged();
        }
    }
}
