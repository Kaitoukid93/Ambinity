using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public static class ColorConfigurationHelper
{
    public static ColorConfigurationViewModelBase  GetColorConfiguration(ILightingConfiguration configuration)
    {
        
        switch (configuration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return new ScreenCaptureConfigurationViewModel(configuration as ScreenCaptureConfiguration);
                break;
            case ConfigurationType.ColorPalette:
                return new ColorPaletteConfigurationViewModel(configuration as ColorPaletteConfiguration);
                break;
            case ConfigurationType.StaticColor:
                return new StaticColorConfigurationViewModel(configuration as StaticColorConfiguration);
                break;
            case ConfigurationType.MusicReactive:
                break;
            case ConfigurationType.Gifxelation:
                break;
            case ConfigurationType.Animation:
                break;
        }

        return null;
    }
}