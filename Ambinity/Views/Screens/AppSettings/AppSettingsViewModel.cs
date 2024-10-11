
using Ambinity.ViewModels;
using AmbinityCore.DataBase;
using Microsoft.VisualBasic;
using Constants = AmbinityCore.Constants;

namespace Ambinity.Views.Screens.AppSettings;

public class AppSettingsViewModel : ViewModelBase
{
    private readonly GeneralSettingsManager _settingsManager;

    public AppSettingsViewModel(GeneralSettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
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

    private void RegisterStartupInformation(bool update = false)
    {
        // if (_runAtStartup)
        //     StartUpManager.AddApplicationToTaskScheduler(Constants.StartupServiceName, StartupDelay, update);
        // else
        // {
        //     StartUpManager.RemoveApplicationFromTaskScheduler(Constants.StartupServiceName);
        // }
    }

    public void Init()
    {
        var settings = _settingsManager.Settings;
        RunAtStartup = settings.AutoStart;
        StartupDelay = settings.AutoStartDelay;
    }
}