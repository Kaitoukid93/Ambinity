using System.Collections.Generic;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.CapturingService;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using ScreenCapture.NET;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class ScreenCaptureConfigurationViewModel : ColorConfigurationViewModelBase
{
    public ScreenCaptureConfigurationViewModel(LightingZone zone,
        ParameterViewModelFactory viewModelFactory)
    {
        _zone = zone;
        _parameterViewModelFactory = viewModelFactory;
       
        Init();
    }
    
    private ParameterViewModelFactory _parameterViewModelFactory;
    private readonly LightingZone _zone;


    /// <summary>
    /// Init list param
    /// </summary>
    public override void Init()
    {
        foreach (var param in _parameterViewModelFactory.CreateParameterViewModels(_zone))
        {
            Parameters.Add(param);
        }
    }
    

}