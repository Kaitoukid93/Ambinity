using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class AnimationConfigurationViewModel : ColorConfigurationViewModelBase
{
    public AnimationConfigurationViewModel(AnimationConfiguration configuration,
        ParameterViewModelFactory parameterViewModelFactory)
    {
        _parameterViewModelFactory = parameterViewModelFactory;
        _configuration = configuration;
        Init();
    }


    private AnimationConfiguration _configuration;
    private readonly ParameterViewModelFactory _parameterViewModelFactory;

    public override void Init()
    {
        
        foreach (var param in _parameterViewModelFactory.CreateParameterViewModels(_configuration))
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