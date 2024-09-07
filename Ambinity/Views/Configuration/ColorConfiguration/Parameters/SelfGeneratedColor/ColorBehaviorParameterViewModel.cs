using System;
using System.Windows.Input;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ColorBehaviorParameterViewModel : ParameterViewModelBase
{
    public ColorBehaviorParameterViewModel(SelfGeneratedColorConfiguration config)
    {
        _blendMode = config.Blend.Mode == PaletteBlendModeEnum.NoBlend ? 0 : 1;
        _configuration = config;
        _isColorMoving = _configuration.IsMoving;
        _speed = _configuration.Speed;
        _resolution = _configuration.ColorResolution * 100 / 8;
        _isReverse = _configuration.IsReverse;
        SetQuickResolutionCommand = new RelayCommand<string>(SetQuickResolution);
    }

    private void SetQuickResolution(string value)
    {
        Resolution = (float)(100 * Convert.ToDecimal(value));
    }

    private SelfGeneratedColorConfiguration _configuration;
    private int _blendMode;

    public int BlendMode
    {
        get => _blendMode;
        set
        {
            _blendMode = value;
            _configuration.Blend.Mode = value == 0 ? PaletteBlendModeEnum.NoBlend : PaletteBlendModeEnum.LinearBlend;
            _configuration.UpdateColors();
            OnPropertyChanged();
        }
    }

    private int _blendValue;

    public int BlendValue
    {
        get => _blendValue;
        set
        {
            _blendValue = value;
            //set value
            OnPropertyChanged();
        }
    }

    private float _speed;

    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            _configuration.Speed = value;
            _configuration.UpdateColorsBehavior();
            OnPropertyChanged();
        }
    }

    private float _resolution;

    public float Resolution
    {
        get => _resolution;
        set
        {
            _resolution = value;
            _configuration.ColorResolution = 8 * value / 100;
            _configuration.UpdateColorsBehavior();
            OnPropertyChanged();
        }
    }

    private bool _isColorMoving;

    public bool IsColorMoving
    {
        get => _isColorMoving;
        set
        {
            _isColorMoving = value;
            _configuration.IsMoving = value;
            _configuration.UpdateColorsBehavior();
            OnPropertyChanged();
        }
    }

    private bool _isReverse;

    public bool IsReverse
    {
        get => _isReverse;
        set
        {
            _isReverse = value;
            _configuration.IsReverse = value;
            _configuration.UpdateColorsApperance();
            OnPropertyChanged();
        }
    }
    public ICommand SetQuickResolutionCommand { get; set; }
}