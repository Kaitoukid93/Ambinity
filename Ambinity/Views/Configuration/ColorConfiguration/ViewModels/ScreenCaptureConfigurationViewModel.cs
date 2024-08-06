using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using DynamicData;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ScreenCaptureConfigurationViewModel : ColorConfigurationViewModelBase
{
    public ScreenCaptureConfigurationViewModel(ScreenCaptureConfiguration configuration)
    {
        _configuration = configuration;
        Init();
    }

    private ScreenCaptureConfiguration _configuration;

    /// <summary>
    /// Init list param
    /// </summary>
    private void Init()
    {
        var captureParameter = new ScreenRegionSelectionParameterViewModel();
        var brightnessParameter = new BrightnessParameterViewModel();
        var staturationParameter = new SaturationParameterViewModel();
        var smoothingParamter = new SmoothingParameterViewModel();
        Parameters.Add(captureParameter);
        Parameters.Add(new SeparationParameterViewModel());
        Parameters.Add(brightnessParameter);
        Parameters.Add(new SeparationParameterViewModel());
        Parameters.Add(staturationParameter);
        Parameters.Add(new SeparationParameterViewModel());
        Parameters.Add(smoothingParamter);
    }
}