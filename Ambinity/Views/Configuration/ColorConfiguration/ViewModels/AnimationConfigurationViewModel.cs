using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class AnimationConfigurationViewModel : ColorConfigurationViewModelBase
{
    public AnimationConfigurationViewModel(LightingZone zone,
        ParameterViewModelFactory parameterViewModelFactory)
    {
        _parameterViewModelFactory = parameterViewModelFactory;
        _zone = zone;
        _configuration = _zone.LightingConfiguration as AnimationConfiguration;
        Init();
    }

    private LightingZone _zone;
    private AnimationConfiguration _configuration;
    private readonly ParameterViewModelFactory _parameterViewModelFactory;

    public override void Init()
    {
        
        foreach (var param in _parameterViewModelFactory.CreateParameterViewModels(_zone))
        {
            Parameters.Add(param);
        }
    }

    public override void Dispose()
    {
        foreach (var param in Parameters)
        {
            param?.Dispose();
        }
        Parameters?.Clear();
    }
}