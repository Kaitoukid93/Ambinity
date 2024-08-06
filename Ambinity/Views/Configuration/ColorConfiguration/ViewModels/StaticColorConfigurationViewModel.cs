using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class StaticColorConfigurationViewModel : ColorConfigurationViewModelBase
{
    public StaticColorConfigurationViewModel(StaticColorConfiguration configuration)
    {
        _configuration = configuration;
        Init();
    }

    private StaticColorConfiguration _configuration;
    private void Init()
    {
        var staticColorSelectorParameter = new StaticColorSelectionParameterViewModel(_configuration.Color);
        Parameters.Add(staticColorSelectorParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var brightnessParameter = new BrightnessParameterViewModel();
        Parameters.Add(brightnessParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var staturationParameter = new SaturationParameterViewModel();
        Parameters.Add(staturationParameter);
        Parameters.Add(new SeparationParameterViewModel());
        var smoothingParamter = new SmoothingParameterViewModel();
        Parameters.Add(smoothingParamter);

      
    }
}