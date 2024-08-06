using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ColorPaletteConfigurationViewModel : ColorConfigurationViewModelBase
{
    public ColorPaletteConfigurationViewModel(ColorPaletteConfiguration configuration)
    {
        _configuration = configuration;
        Init();
    }

    private ColorPaletteConfiguration _configuration;

    private void Init()
    {
        var paletteSelectionParameter = new PaletteSelectionParameterViewModel(_configuration.Palette);
        Parameters.Add(paletteSelectionParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var paletteAppearanceParameter = new PaletteAppearanceParameterViewModel(_configuration.Apperance);
        Parameters.Add(paletteAppearanceParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var paletteBlendModeParameter = new PaletteBlendModeParameterViewModel(_configuration.Blend);
        Parameters.Add(paletteBlendModeParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var brightnessParameter = new BrightnessParameterViewModel();
        Parameters.Add(brightnessParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var paletteIntensityParameter = new PaletteColorIntensityViewModel();
        Parameters.Add(paletteIntensityParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var speedParameterViewModel = new SpeedParameterViewModel();
        Parameters.Add(speedParameterViewModel);
    }
}