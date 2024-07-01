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
    private Color _primaryColor = Colors.MediumSlateBlue;

    public Color PrimaryColor
    {
        get => _primaryColor;
        set => SetProperty(ref _primaryColor, value);
    }
}