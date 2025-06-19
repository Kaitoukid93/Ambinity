using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.SystemUtilities;
using Ambinity.Utils;
using Ambinity.ViewModels;
using Ambinity.Views.Debug;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Utils;
using AmbinityServer.AppRelease;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using Microsoft.VisualBasic;
using Serilog;
using Constants = AmbinityCore.Constants;

namespace Ambinity.Views.Screens.AppSettings;

public class AppSettingsViewModel : ViewModelBase
{
    private const string _system = "System";
    private const string _dark = "Dark";
    private const string _light = "Light";
    private readonly DebugWindowViewModel _debugWindowViewModel;
    private readonly IWindowService _windowService;
    private readonly GeneralSettingsManager _settingsManager;
    private IGeneralSettings _generalSettings;
    private UpdateService _updateService;
    public IGeneralSettings GeneralSettings => _generalSettings;
    private IProgress<int> _updatingProgress;
    private AppThemeManager _appThemeManager;

    public AppSettingsViewModel(GeneralSettingsManager settingsManager, IWindowService windowService, DebugWindowViewModel debugWindowViewModel, UpdateService updateService, AppThemeManager appThemeManager)
    {
        _debugWindowViewModel = debugWindowViewModel;
        _windowService = windowService;
        _settingsManager = settingsManager;
        _updateService = updateService;
        var settings = _settingsManager.Settings;
        _generalSettings = settings;
        _generalSettings.PropertyChanged += OnGeneralSettingsPropertyChanged;
        _useSystemTheme = _generalSettings.SelectedTheme == "System";
        _currentAppTheme = _generalSettings.SelectedTheme;
        RunAtStartup = _generalSettings.AutoStart;
        _startupDelay = _generalSettings.AutoStartDelay;
        _enableMica = _generalSettings.EnableMica;
        _startMinimized = _generalSettings.StartMinimized;
        _autoScanNewDevices = _generalSettings.AutoScanNewDevices;
        _appThemeManager = appThemeManager;
        AvailableFrameRates = ["24 FPS", "30 FPS", "60 FPS", "100 FPS", "144 FPS"];
        AvailableBitmapSize = ["400 * 320 px", "750 * 500 px", "800 * 600 px", "1024 * 768 px", "1200 * 600 px", "1000 * 500 px"];
        _targetBitmapSize = AvailableBitmapSize.Where(f =>
                f.Contains(_generalSettings.CanvasWidth.ToString()) &&
                f.Contains(_generalSettings.CanvasHeight.ToString()))
            .First();
        _targetFramerate = AvailableFrameRates.Where(f => f.Contains(_generalSettings.TargetFramerate.ToString()))
            .First();
        RequestRestartApplicationCommand = new RelayCommand(RequestRestartApplication);
        CheckForAppUpdateCommand = new AsyncRelayCommand(CheckForAppUpdate);
        RemoveAllDevicesCommand = new RelayCommand(RemoveAllDevices);
        RemoveDownloadedDatasCommand = new RelayCommand(RemoveDownloadedDatas);
        ShowDebugLogCommand = new RelayCommand(ShowDebugLog);
        var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        CurrentReleaseInformation = new AppReleaseInformation(assemblyVersion, DateTime.Now);
        InstallUpdateCommand = new AsyncRelayCommand(InstallUpdate);
        _updatingProgress = new Progress<int>((p) => { CurrentUpdateProgress = p; });
    }


    public bool HWMonitorSettingVisible { get; } = OperatingSystem.IsWindows();
    public bool OpenRGBSettingVisible { get; } = OperatingSystem.IsWindows();
    public bool EnableInAppUpdate { get; } = OperatingSystem.IsWindows();
    private int _currentUpdateProgress;

    public int CurrentUpdateProgress
    {
        get => _currentUpdateProgress;
        set
        {
            _currentUpdateProgress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// updating process learning from ArtemisRGB
    /// </summary>
    private async Task InstallUpdate()
    {
        IsUpdating = true;
        //download release
        CurrentUpdatingStatus = "Downloading update...";
        var (zipFile, release) = await _updateService.DownloadRelease(_updatingProgress);
        //extract release
        CurrentUpdatingStatus = "Extracting...";
        await _updateService.ExtractRelease(zipFile, _updatingProgress);
        // Remove the installer archive
        File.Delete(zipFile);
        //copy script file
        // var dir = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
        // var scriptDir = Path.Combine(dir, "Scripts", "update.ps1");
        // if (!File.Exists(scriptDir))
        // {
        //     Log.Error("Script file not found, aborting...");
        //     CurrentUpdatingStatus = "Script file not found, aborting...";
        //     return;
        // }
        //
        // Directory.CreateDirectory(Path.Combine(Constants.UpdatingFolder, "installing", "scripts"));
        // File.Copy(scriptDir, Path.Combine(Constants.UpdatingFolder, "installing", "scripts", "update.ps1"));
        //restart app for updating with script
        CurrentUpdatingStatus = "Restarting...";
        Utilities.ApplyUpdate(true);
        IsUpdating = false;
    }
    private void ShowDebugLog()
    {
        _debugWindowViewModel.Init();

        _windowService.ShowWindow(_debugWindowViewModel);
    }
    private void RemoveDownloadedDatas()
    {
        AppUtilities.RemoveAppData();
        RequestRestartApplication();
    }
    private void RemoveAllDevices()
    {
        AppUtilities.RemoveAllDevices();
        RequestRestartApplication();
    }
    private async Task CheckForAppUpdate()
    {
        IsCheckingForUpdate = true;
        UpdateAvailable = false;
        // check for last update, gotta fake delay cuz this thing run too fast
        await Task.Delay(2000);
        var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
        var latestRelease = await _updateService.GetLatestRelease();
        if (latestRelease != null)
        {
            if (assemblyVersion < new Version(latestRelease.Version))
            {
                UpdateAvailable = true;
                UpdateInformation = latestRelease;
            }
            else
            {
                UpdateAvailable = false;
                UpdateInformation = null;
            }
        }

        IsCheckingForUpdate = false;
    }

    private AppReleaseInformation _currentReleaseInformation;

    public AppReleaseInformation CurrentReleaseInformation
    {
        get => _currentReleaseInformation;
        set
        {
            _currentReleaseInformation = value;
            OnPropertyChanged();
        }
    }

    private AppReleaseInformation _updateInformation;

    public AppReleaseInformation UpdateInformation
    {
        get => _updateInformation;
        set
        {
            _updateInformation = value;
            OnPropertyChanged();
        }
    }

    private bool _updateAvailable;

    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set
        {
            _updateAvailable = value;
            OnPropertyChanged();
        }
    }

    private bool _isCheckingForUpdate;

    public bool IsCheckingForUpdate
    {
        get => _isCheckingForUpdate;
        set
        {
            _isCheckingForUpdate = value;
            OnPropertyChanged();
        }
    }
    public string BuildInfo { get; } = GetBuildInfo();

    private static string GetBuildInfo()
    {
        var buildNumber = DateTime.UtcNow.ToString("yyyyMMdd");
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "?.?.?.?";
        var os = RuntimeInformation.RuntimeIdentifier;
        return $"Build: {buildNumber} | Version: {version} | OS: {os}";
    }
    public AsyncRelayCommand CheckForAppUpdateCommand { get; set; }

    private void RequestRestartApplication()
    {
        Utilities.Restart(false, TimeSpan.FromSeconds(1), new string[0]);
    }

    private void OnGeneralSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_generalSettings.EnableScreenCapture):
            case nameof(_generalSettings.EnableHWMonitor):
            case nameof(_generalSettings.EnableAudioCapture):
            case nameof(_generalSettings.EnableOpenRGB):
                ShowPluginInfoBar = true;
                break;
            case nameof(_generalSettings.CanvasWidth):
            case nameof(_generalSettings.CanvasHeight):
                ShowRenderingInfoBar = true;
                break;
            case nameof(_generalSettings.AutoScanNewDevices):
                ShowDevicesInfoBar = true;
                break;
        }
    }


    private bool _runAtStartup;

    public bool RunAtStartup
    {
        get => _runAtStartup;
        set
        {
            _runAtStartup = value;
            if(OperatingSystem.IsWindows())
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
        new[] { _light, _dark /*, FluentAvaloniaTheme.HighContrastTheme*/ };

    private string _currentAppTheme = _system;

    public string CurrentAppTheme
    {
        get => _currentAppTheme;
        set
        {
            _currentAppTheme = value;
            _generalSettings.SelectedTheme = value;
            OnPropertyChanged();
            if (value != null && value != string.Empty)
            {
                _appThemeManager.SetAppTheme(value);
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
            _appThemeManager.UpdateAppAccentColor(value);
            OnPropertyChanged();
        }
    }

    private bool _showPluginInfoBar;

    public bool ShowPluginInfoBar
    {
        get => _showPluginInfoBar;
        set
        {
            _showPluginInfoBar = value;
            OnPropertyChanged();
        }
    }

    public ICommand RequestRestartApplicationCommand { get; }
    public ICommand RemoveAllDevicesCommand { get; }
    public ICommand ShowDebugLogCommand { get; }
    public ICommand RemoveDownloadedDatasCommand { get; }

    private void RegisterStartupInformation(bool update = false)
    {
        if (_runAtStartup)
            StartUpManager.AddApplicationToTaskScheduler(Constants.StartupServiceName, StartupDelay, update);
        else
        {
            StartUpManager.RemoveApplicationFromTaskScheduler(Constants.StartupServiceName);
        }
    }

    public List<string> AvailableFrameRates { get; set; }

    private string _targetFramerate;

    public string TargetFramerate
    {
        get => _targetFramerate;
        set
        {
            _targetFramerate = value;
            _generalSettings.TargetFramerate = FrameRateConverter(value);
            OnPropertyChanged();
        }
    }
    private bool _autoScanNewDevices;
    public bool AutoScanNewDevices
    {
        get => _autoScanNewDevices;
        set
        {
            _autoScanNewDevices = value;
            _generalSettings.AutoScanNewDevices = value;
            ShowDevicesInfoBar = true;
            OnPropertyChanged();
        }
    }

    public List<string> AvailableBitmapSize { get; set; }

    private int FrameRateConverter(string frameRate)
    {
        switch (frameRate)
        {
            case "24 FPS":
                return 30;
            case "30 FPS":
                return 30;
                break;
            case "60 FPS":
                return 60;
                break;
            case "100 FPS":
                return 100;
                break;
            case "144 FPS":
                return 144;
                break;
            default:
                return 30;
        }
    }

    private string _targetBitmapSize;

    public string TargetBitmapSize
    {
        get => _targetBitmapSize;
        set
        {
            _targetBitmapSize = value;
            _generalSettings.CanvasWidth = (int)BitmapSizeConverter(value).Width;
            _generalSettings.CanvasHeight = (int)BitmapSizeConverter(value).Height;
            OnPropertyChanged();
        }
    }

    private bool _showRenderingInfoBar;

    public bool ShowRenderingInfoBar
    {
        get => _showRenderingInfoBar;
        set
        {
            _showRenderingInfoBar = value;
            OnPropertyChanged();
        }
    }
    private bool _showDevicesInfoBar;

    public bool ShowDevicesInfoBar
    {
        get => _showDevicesInfoBar;
        set
        {
            _showDevicesInfoBar = value;
            OnPropertyChanged();
        }
    }

    public ICommand InstallUpdateCommand { get; set; }
    private string _currentUpdatingStatus;

    public string CurrentUpdatingStatus
    {
        get => _currentUpdatingStatus;
        set
        {
            _currentUpdatingStatus = value;
            OnPropertyChanged();
        }
    }

    private bool _isUpdating;

    public bool IsUpdating
    {
        get => _isUpdating;
        set
        {
            _isUpdating = value;
            OnPropertyChanged();
        }
    }

    private Size BitmapSizeConverter(string size)
    {
        switch (size)
        {
            case "400 * 320 px":
                return new Size(400, 320);
            case "750 * 500 px":
                return new Size(750, 500);
                break;
            case "800 * 600 px":
                return new Size(800, 600);
                break;
            case "1024 * 768 px":
                return new Size(1024, 768);
                break;
            case "1200 * 600 px":
                return new Size(1200, 600);
                break;
            case "1000 * 500 px":
                return new Size(1000, 500);
                break;
            default:
                return new Size(750, 500);
        }
    }

    public void Init()
    {
    }
}
