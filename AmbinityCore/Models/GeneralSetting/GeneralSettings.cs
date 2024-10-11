using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.GeneralSetting;

public class GeneralSettings : ObservableObject, IGeneralSettings
{
    private bool _autoStart = true;

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
    private Color _primaryColor = Avalonia.Media.Colors.LimeGreen;

    public Color PrimaryColor
    {
        get => _primaryColor;
        set => SetProperty(ref _primaryColor, value);
    }

    private bool _enableSnapToGrid = true;

    public bool EnableSnapToGrid
    {
        get => _enableSnapToGrid;
        set => SetProperty(ref _enableSnapToGrid, value);
    }

    private bool _startMinimized = true;

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

    private bool _showApptour = true;
    public bool ShowAppTour
    {
        get => _showApptour;
        set => SetProperty(ref _showApptour, value);

    }

    private Guid _lastPlayedProfileID;
    public Guid LastPlayedProfileID  {
        get => _lastPlayedProfileID;
        set => SetProperty(ref _lastPlayedProfileID, value);

    }
}