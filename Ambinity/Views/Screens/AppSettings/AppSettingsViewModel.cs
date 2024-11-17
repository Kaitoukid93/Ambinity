
using Ambinity.Installer;
using Ambinity.SystemUtilities;
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using Microsoft.VisualBasic;
using Constants = AmbinityCore.Constants;

namespace Ambinity.Views.Screens.AppSettings;

public class AppSettingsViewModel : ViewModelBase
{
    private const string _system = "System";
    private const string _dark = "Dark";
    private const string _light = "Light";
    private readonly GeneralSettingsManager _settingsManager;
    private  IGeneralSettings _generalSettings;
    public AppSettingsViewModel(GeneralSettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        
        var settings = _settingsManager.Settings;
        _generalSettings = settings;
        _useSystemTheme = _generalSettings.SelectedTheme == "System";
        _currentAppTheme = _generalSettings.SelectedTheme;
        _runAtStartup = _generalSettings.AutoStart;
        _startupDelay = _generalSettings.AutoStartDelay;
        _enableMica = _generalSettings.EnableMica;
    }


    private bool _runAtStartup;

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set
        {
            _runAtStartup = value;
            RegisterStartupInformation();
            OnPropertyChanged();
        }
    }

    private int _startupDelay;

    public int StartupDelay
    {
        get => _startupDelay;
        set
        {
            _startupDelay = value;
            RegisterStartupInformation(true);
            OnPropertyChanged();
        }
    }

    private bool _enableMica;

    public bool EnableMica
    {
        get => _enableMica;
        set
        {
            _enableMica = value;
            _generalSettings.EnableMica = value;
            OnPropertyChanged();
        }
    }
    private bool _useSystemTheme;

    public bool UseSystemTheme
    {
        get => _useSystemTheme;
        set
        {
            _useSystemTheme = value;
            if (value)
                CurrentAppTheme = _system;
            else
            {
                _currentAppTheme = _dark;
            }
            OnPropertyChanged();
        }
    }
    private bool _startMinimized;

    public bool StartMinimized
    {
        get => _startMinimized;
        set
        {
            _startMinimized = value;
            _generalSettings.StartMinimized = value;
            OnPropertyChanged();
        }
    }

   
    public string[] AvailableAppThemes { get; } =
        new[] { _light , _dark /*, FluentAvaloniaTheme.HighContrastTheme*/ };

    private string _currentAppTheme = _system;
    public string CurrentAppTheme
    {
        get => _currentAppTheme;
        set
        {
            _currentAppTheme = value;
            _generalSettings.SelectedTheme = value;
            OnPropertyChanged();
                if (value != null&& value!= string.Empty)
                {
                   AppThemeManager.SetAppTheme(value);
                }
        }
    }

    private Color _currentAppAccentColor;

    public Color CurrentAppAccentColor
    {
        get => _currentAppAccentColor;
        set
        {
            _currentAppAccentColor = value;
            _generalSettings.PrimaryColor = value;
            AppThemeManager.UpdateAppAccentColor(value);
            OnPropertyChanged();
        }
    }

    private void RegisterStartupInformation(bool update = false)
    {
        if (_runAtStartup)
            StartUpManager.AddApplicationToTaskScheduler(Constants.StartupServiceName, StartupDelay, update);
        else
        {
            StartUpManager.RemoveApplicationFromTaskScheduler(Constants.StartupServiceName);
        }
    }

    public void Init()
    {
        
    }
}