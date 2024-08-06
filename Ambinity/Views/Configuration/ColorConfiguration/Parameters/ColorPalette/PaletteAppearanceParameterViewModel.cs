using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class PaletteAppearanceParameterViewModel : ParameterViewModelBase
{
    public PaletteAppearanceParameterViewModel(ColorApperance appearance)
    {
        Appearance = appearance.Mode == ColorApperanceEnum.Fill ? 1 : 0;
        AppearanceValue = appearance.Value;
    }

    private int _appearance;

    public int Appearance
    {
        get => _appearance;
        set
        {
            _appearance = value;
            if (value == 0)
            {
                ValueSuffix = "º";
                Icon = "Rotate_value";
            }
            
            
            else
            {
                ValueSuffix = "px";
                Icon = "Stroke_value";
            }

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