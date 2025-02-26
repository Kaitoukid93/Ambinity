using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Color = Avalonia.Media.Color;

namespace AmbinityCore.Models.GeneralSetting;

public class GeneralSettings : ObservableObject, IGeneralSettings
{
    private bool _autoStart = true;

    #region General

    public bool AutoStart
    {
        get => _autoStart;
        set => SetProperty(ref _autoStart, value);
    }

    private int _autoStartDelay = 5;

    public int AutoStartDelay
    {
        get => _autoStartDelay;
        set => SetProperty(ref _autoStartDelay, value);
    }

    private Color _primaryColor = Color.Parse("#1ac35f");


    public Color PrimaryColor
    {
        get => _primaryColor;
        set => SetProperty(ref _primaryColor, value);
    }

    private bool _enableMica = false;

    public bool EnableMica
    {
        get => _enableMica;
        set => SetProperty(ref _enableMica, value);
    }

    private string _selectedTheme = "System";

    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
    }

    #endregion

    #region Canvas

    private bool _enableSnapToGrid = true;

    public bool EnableSnapToGrid
    {
        get => _enableSnapToGrid;
        set => SetProperty(ref _enableSnapToGrid, value);
    }

    private bool _startMinimized = false;

    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetProperty(ref _startMinimized, value);
    }

    private bool _showCanvasLockedInfo = true;

    public bool ShowCanvasLockedInfo
    {
        get => _showCanvasLockedInfo;
        set => SetProperty(ref _showCanvasLockedInfo, value);
    }

    #endregion

    #region Plugin

    private bool _enableScreenCapture = true;

    public bool EnableScreenCapture
    {
        get => _enableScreenCapture;
        set => SetProperty(ref _enableScreenCapture, value);
    }

    private bool _enableAudioCapture = true;

    public bool EnableAudioCapture
    {
        get => _enableAudioCapture;
        set => SetProperty(ref _enableAudioCapture, value);
    }

    private bool _enableHWMonitor = true;

    public bool EnableHWMonitor
    {
        get => _enableHWMonitor;
        set => SetProperty(ref _enableHWMonitor, value);
    }

    private bool _enableOpenRGB = false;

    public bool EnableOpenRGB
    {
        get => _enableOpenRGB;
        set => SetProperty(ref _enableOpenRGB, value);
    }

    #endregion


    #region Profile

    private int _canvasWidth =1000;

    public int CanvasWidth
    {
        get =>_canvasWidth ;
        set => SetProperty(ref _canvasWidth, value);
    }
    private int _canvasHeight =500;

    public int CanvasHeight
    {
        get =>_canvasHeight ;
        set => SetProperty(ref _canvasHeight, value);
    }
    private int _targetFramerate = 30;

    public int TargetFramerate
    {
        get => _targetFramerate;
        set => SetProperty(ref _targetFramerate, value);
    }

    private Guid _lastPlayedProfileID;

    public Guid LastPlayedProfileID
    {
        get => _lastPlayedProfileID;
        set => SetProperty(ref _lastPlayedProfileID, value);
    }

    #endregion

    #region First Time

    private bool _showApptour = true;

    public bool ShowAppTour
    {
        get => _showApptour;
        set => SetProperty(ref _showApptour, value);
    }

    #endregion
}