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
}