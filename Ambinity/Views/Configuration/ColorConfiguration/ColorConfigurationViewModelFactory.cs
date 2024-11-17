using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ColorConfigurationViewModelFactory
{
    private readonly ParameterViewModelFactory _parameterViewModelFactory;

    public ColorConfigurationViewModelFactory(ParameterViewModelFactory parameterViewModelFactory)
    {
        _parameterViewModelFactory = parameterViewModelFactory;
    }

    public ColorConfigurationViewModelBase GetColorConfiguration(LightingZone zone)
    {
        switch (zone.LightingConfiguration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return new ScreenCaptureConfigurationViewModel(zone,_parameterViewModelFactory);
                break;

            case ConfigurationType.SelfGeneratedColor:
                return new SelfColorGenerationConfigurationViewModel(zone,_parameterViewModelFactory);
                break;
            case ConfigurationType.Animation:
                return new AnimationConfigurationViewModel(zone, _parameterViewModelFactory);
                break;
        }

        return null;
    }
}