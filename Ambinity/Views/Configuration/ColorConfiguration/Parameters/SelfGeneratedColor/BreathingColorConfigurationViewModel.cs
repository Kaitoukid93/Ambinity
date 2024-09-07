using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class BreathingColorConfigurationViewModel : MotionConfigurationViewModelBase
{
    private readonly BreathingMotionConfiguration? _configuration;

    public BreathingColorConfigurationViewModel(IMotionConfiguration config)
    {
        Configuration = config;
        _configuration = config as BreathingMotionConfiguration;
        Type = config.Type;
        Icon = Configuration.Icon;
        Name = "Breathing";
        Description = "Brightness change with sin wave";
        ShowSettingsButton = true;
        _breathingSpeed = (double)_configuration.BreathingSpeed;
        _isSystemSync = _configuration.IsSystemSync;
    }

    public string Icon { get; set; }
    private double _breathingSpeed;

    public double BreathingSpeed
    {
        get => _breathingSpeed;
        set
        {
            if(_breathingSpeed ==value)
                return;
            _breathingSpeed = value;
            _configuration.BreathingSpeed = (float)value;
            _configuration.UpdateSpeed();
            OnPropertyChanged();
        }
    }

    private bool _isSystemSync;

    public bool IsSystemSync
    {
        get => _isSystemSync;
        set
        {
            _isSystemSync = value;
            _configuration.IsSystemSync = value;
            _configuration.UpdateSystemSync();
            OnPropertyChanged();
        }
    }
}