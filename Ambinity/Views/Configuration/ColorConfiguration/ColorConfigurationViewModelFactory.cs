using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ColorConfigurationViewModelFactory
{
    private readonly ParameterViewModelFactory _parameterViewModelFactory;

    public ColorConfigurationViewModelFactory(ParameterViewModelFactory parameterViewModelFactory)
    {
        _parameterViewModelFactory = parameterViewModelFactory;
    }

    public ColorConfigurationViewModelBase GetColorConfiguration(ILightingConfiguration configuration)
    {
        switch (configuration.Type)
        {
            case ConfigurationType.ScreenCapture:
                return new ScreenCaptureConfigurationViewModel(configuration as ScreenCaptureConfiguration,_parameterViewModelFactory);
                break;

            case ConfigurationType.SelfGeneratedColor:
                return new SelfColorGenerationConfigurationViewModel(configuration as SelfGeneratedColorConfiguration,_parameterViewModelFactory);
                break;
            case ConfigurationType.Animation:
                return new AnimationConfigurationViewModel(configuration as AnimationConfiguration, _parameterViewModelFactory);
                break;
        }

        return null;
    }
}