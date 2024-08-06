using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class PaletteBlendModeParameterViewModel : ParameterViewModelBase
{
    public PaletteBlendModeParameterViewModel(PaletteBlend blend)
    {
        BlendMode = blend.Mode == PaletteBlendModeEnum.NoBlend ? 0 : 1;
        BlendValue = blend.Value;
    }

    private int _blendMode;

    public int BlendMode
    {
        get => _blendMode;
        set
        {
            _blendMode = value;
            ShowBlendValue = value == 0 ? false : true;

            //set blend mode and value
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

    private bool _showBlendValue;

    public bool ShowBlendValue
    {
        get => _showBlendValue;
        set
        {
            _showBlendValue = value;
            OnPropertyChanged();
        }
    }
}