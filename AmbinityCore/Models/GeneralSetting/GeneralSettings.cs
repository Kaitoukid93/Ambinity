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
    private Color _primaryColor = Avalonia.Media.Colors.Lime;

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
}