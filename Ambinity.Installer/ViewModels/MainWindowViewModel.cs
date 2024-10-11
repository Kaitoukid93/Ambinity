using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;

namespace Ambinity.Installer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(SecondStepViewModel secondStepViewModel,FirstStepViewModel firstStepViewModel,WelcomeViewModel welcomeViewModel,ThirdStepViewModel thirdStepViewModel)
    {
        Header = "Ambinity Installer";
        NextButtonContent = "Next";
        NextCommand = new RelayCommand(NexStep);
        CancelCommand = new RelayCommand(CancelSetup);
        FinishCommand = new RelayCommand(FinishSetup);
        BackCommand = new RelayCommand(PreviousStep);
        thirdStepViewModel.AccentColorChanged += OnAccentColorChanged;
        Steps =
        [
            welcomeViewModel,
            firstStepViewModel,
            secondStepViewModel,
            thirdStepViewModel
        ];
        
    }

    public void Init()
    {
        _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        AccentColor = _faTheme.CustomAccentColor;
        UpdateAccentColor();
        CurrentView = Steps.First();
    }
    private void OnAccentColorChanged(Color color)
    {
        AccentColor = color;
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

    private Color? _accentColor;

    public Color? AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = value;
            UpdateAccentColor();
            OnPropertyChanged();
        }
    }

    private void UpdateAccentColor()
    {
        _faTheme = App.Current?.Styles[0] as FluentAvaloniaTheme;
        _faTheme.CustomAccentColor = AccentColor;
        AmbinityImage = new AmbinityImageViewModel(new SolidColorBrush(AccentColor ?? Colors.LimeGreen));
    }

    private void FinishSetup()
    {
        //
    }


    private void CancelSetup()
    {
        //dispose and close
    }


    private void NexStep()
    {
        if (CurrentView.StepIndex == Steps.Count - 1)
            return;
        CurrentView = Steps[CurrentView.StepIndex + 1];
    }

    private void PreviousStep()
    {
        if (CurrentView.StepIndex != 1)
            return;
        CurrentView = Steps[0];
    }

    public string Header { get; set; }
    public string NextButtonContent { get; }
    private StepViewModelBase _currentView;
    private FluentAvaloniaTheme? _faTheme;

    public StepViewModelBase CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public List<StepViewModelBase> Steps { get; set; }

    public ICommand NextCommand { get; set; }
    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
}