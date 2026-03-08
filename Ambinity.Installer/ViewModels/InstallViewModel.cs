using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ambinity.Installer.Models;
using Ambinity.Installer.Services;
using Ambinity.Installer.Utilities;
using AmbinityServer.AppRelease;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using Newtonsoft.Json;
using Ambinity.Installer.Localization;

namespace Ambinity.Installer.ViewModels;

public class InstallViewModel : ViewModelBase
{
    public InstallViewModel(SecondStepViewModel secondStepViewModel, FirstStepViewModel firstStepViewModel,
        WelcomeViewModel welcomeViewModel, ThirdStepViewModel thirdStepViewModel, PostInstallationSettings settings,
        InstallationService installationService)
    {
        _installationService = installationService;
        _postInstallationSettings = settings;

        NextCommand = new RelayCommand(NexStep);
        CancelCommand = new RelayCommand(CancelSetup);
        FinishCommand = new RelayCommand(FinishSetup);
        BackCommand = new RelayCommand(PreviousStep);
        thirdStepViewModel.AccentColorChanged += OnAccentColorChanged;
        secondStepViewModel.AccentColorChanged += OnAccentColorChanged;
        //init steps based on parameter
        Steps =
        [
            welcomeViewModel,
            firstStepViewModel,
            secondStepViewModel,
            thirdStepViewModel
        ];
        Loc.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(NextButtonContent));
        OnPropertyChanged(nameof(BackButtonContent));
        OnPropertyChanged(nameof(CancelButtonContent));
        _postInstallationSettings.SelectedLanguage = Loc.CurrentLanguage;
    }
    public event Action CloseWindowRequested;
    private PostInstallationSettings _postInstallationSettings;

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
        if (!File.Exists(Constants.GeneralSettingsFilePath))
            SaveInitialAppSettings();
        if (_postInstallationSettings.CreateDesktopShortcut)
            _installationService.CreateDesktopShortcut();
        if (_postInstallationSettings.OpenAfterFinish)
        {
            string executable = Path.Combine(_installationService.InstallationDirectory, "Ambinity.Windows.exe");
            ProcessUtilities.RunAsDesktopUser(executable);
        }

        CloseWindowRequested?.Invoke();
    }

    public void InstallCustomVersion(AppReleaseInformation versionInfomation)
    {
        CurrentView = Steps[2];
        (_currentView as SecondStepViewModel).Init(versionInfomation);

    }
    private void CancelSetup()
    {
        CloseWindowRequested?.Invoke();
    }

    private void SaveInitialAppSettings()
    {
        try
        {
            var initialSettings = new GeneralSettings();
            initialSettings.AutoStart = _postInstallationSettings.AutoStart;
            initialSettings.PrimaryColor = _postInstallationSettings.PrimaryColor;
            initialSettings.EnableMica = false;
            initialSettings.SelectedTheme = "Dark";
            initialSettings.SelectedLanguage = _postInstallationSettings.SelectedLanguage;
            var json = JsonConvert.SerializeObject(initialSettings,
                new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText(Constants.GeneralSettingsFilePath, json);
        }
        catch (Exception ex)
        {
            //log
        }
    }

    //minimal requirement for general settings
    public class    GeneralSettings
    {
        public bool AutoStart { get; set; }
        public Color PrimaryColor { get; set; }
        public string SelectedTheme { get; set; } = "Dark";
        public bool EnableMica { get; set; } = false;
        public string SelectedLanguage { get; set; } = "en";
    }

    private void NexStep()
    {
        if (CurrentView.StepIndex == Steps.Count - 1)
            return;
        CurrentView = Steps[CurrentView.StepIndex + 1];
        if (CurrentView is SecondStepViewModel secondStepViewModel)
        {
            secondStepViewModel.Init();
        }

        if (CurrentView is ThirdStepViewModel thirdStepViewModel)
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

    public string Header => Loc.Get("Install.Header.Content");
    public string NextButtonContent => Loc.Get("Install.NextButton.Content");
    public string BackButtonContent => Loc.Get("Install.BackButton.Content");
    public string CancelButtonContent => Loc.Get("Cancel.Button.Content");
    private StepViewModelBase _currentView;
    private FluentAvaloniaTheme? _faTheme;
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

    public List<StepViewModelBase> Steps { get; set; }

    public ICommand NextCommand { get; set; }
    public ICommand CancelCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
}
