using System;
using System.Collections.Generic;
using System.Windows.Input;
using Ambinity.Installer.Views;
using AmbinityServer.AppRelease;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Installer.Localization;
using System.IO;
using Newtonsoft.Json;
using Ambinity.Installer.Models;

namespace Ambinity.Installer.ViewModels;

public class ModifyViewModel : ViewModelBase
{
    public event Action<string> ModifyOptionCommited;
    public event Action<AppReleaseInformation> CustomVersionSelected;
    private SelectVersionViewModel _selectVersionViewModel;
    public ModifyViewModel(FirstStepModifyViewModel firstStepViewModel, SelectVersionViewModel selectVersionViewModel)
    {
        _selectVersionViewModel = selectVersionViewModel;
        _currentView = firstStepViewModel;
        NextCommand = new RelayCommand(NexStep);
        BackCommand = new RelayCommand(PreviousStep);
        AmbinityImage = new AmbinityImageViewModel(new SolidColorBrush(Colors.LightGray));
        Steps =
        [
            firstStepViewModel,
            selectVersionViewModel,
        ];
          //check if ambinity config exist and read the language settings
        if (File.Exists(Constants.GeneralSettingsFilePath))
        {
            var d = File.ReadAllText(Constants.GeneralSettingsFilePath);
            var settings = JsonConvert.DeserializeObject<PreInstallationSettings>(d);
            var l = settings?.SelectedLanguage;
            if(l!=null)
                Loc.Load(l);
        }
    }
    private StepViewModelBase _currentView;
    public List<StepViewModelBase> Steps { get; set; }
    private void PreviousStep()
    {
        if (CurrentView.StepIndex != 1)
            return;
        CurrentView = Steps[0];
    }
    private void NexStep()
    {
        if (_currentView is FirstStepModifyViewModel vm)
        {
            if (vm.SelectedOption != "custom")
            {
                ModifyOptionCommited?.Invoke(vm.SelectedOption);
            }
            else
            {
                _selectVersionViewModel?.Init();
                CurrentView = _selectVersionViewModel;

            }

        }

        else if (_currentView is SelectVersionViewModel)
        {
            //download and install custom
            CustomVersionSelected?.Invoke(_selectVersionViewModel.SelectedVersion);
        }

    }
    public StepViewModelBase CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }
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
    public string NextButtonContent => Loc.Get("Modify.NextButton.Content");
    private string _selectedOption = String.Empty;

    public ICommand NextCommand { get; set; }
    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
}
