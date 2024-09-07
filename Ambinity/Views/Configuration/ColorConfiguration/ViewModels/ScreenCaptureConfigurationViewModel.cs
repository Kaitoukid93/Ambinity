using System.Collections.Generic;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using ScreenCapture.NET;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ScreenCaptureConfigurationViewModel : ColorConfigurationViewModelBase
{
    public ScreenCaptureConfigurationViewModel(ScreenCaptureConfiguration configuration,
        ParameterViewModelFactory viewModelFactory)
    {
        _configuration = configuration;
        _parameterViewModelFactory = viewModelFactory;
       
        Init();
    }

    private ILightingConfiguration _configuration;
    private ParameterViewModelFactory _parameterViewModelFactory;
   

    /// <summary>
    /// Init list param
    /// </summary>
    public override void Init()
    {
        foreach (var param in _parameterViewModelFactory.CreateParameterViewModels(_configuration))
        {
            Parameters.Add(param);
        }
    }
    

}