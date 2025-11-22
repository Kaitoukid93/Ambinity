using System.Collections.Generic;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ColorAppearanceParameterViewModel : ParameterViewModelBase
{
    public ColorAppearanceParameterViewModel(SelfGeneratedColorConfiguration config, LightingZone zone)
    {
        _configuration = config;
        AppearanceModes = new List<string>();
        if (zone.Shape == ZoneShapeEnum.Rectangle)
            AppearanceModes.Add("Fill entire zone");
        if (zone.Shape == ZoneShapeEnum.Polyline)
        AppearanceModes.Add("Apply stroke only");

        _appearance = _configuration.Apperance.Mode == ColorApperanceEnum.Fill
            ? "Fill entire zone"
            : "Apply stroke only";
        _appearanceValue = _configuration.Apperance.Value;
        _valueSuffix = _appearance == "Fill entire zone" ? "º" : "px";
        _icon = _appearance == "Fill entire zone" ? "Rotate_value" : "Stroke_value";
        _icon = _configuration.Apperance.Mode == ColorApperanceEnum.Fill ? "Rotate_value" : "Stroke_value";
    }

    private SelfGeneratedColorConfiguration _configuration;
    public List<string> AppearanceModes { get; set; }
    private string _appearance;

    private void UpdateAppearance()
    {
        if (_appearance == "Fill entire zone")
        {
            ValueSuffix = "º";
            Icon = "Rotate_value";
            _configuration.Apperance.Mode = ColorApperanceEnum.Fill;
            AppearanceValue = 90;
        }


        else
        {
            ValueSuffix = "px";
            Icon = "Stroke_value";
            _configuration.Apperance.Mode = ColorApperanceEnum.Stroke;
            AppearanceValue = 3;
        }
    }

    public string Appearance
    {
        get => _appearance;
        set
        {
            _appearance = value;
            UpdateAppearance();
            //set apperance
            OnPropertyChanged();
        }
    }

    private int _appearanceValue;

    public int AppearanceValue
    {
        get => _appearanceValue;
        set
        {
            _appearanceValue = value;
            //set value
            OnPropertyChanged();
            _configuration.Apperance.Value = value;
            _configuration.UpdateColorsApperance();
        }
    }

    private string _valueSuffix;

    public string ValueSuffix
    {
        get => _valueSuffix;
        set
        {
            _valueSuffix = value;
            OnPropertyChanged();
        }
    }

    private string _icon;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }
}
