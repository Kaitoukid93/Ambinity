using System.Windows.Input;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class SelfColorGenerationConfigurationViewModel : ColorConfigurationViewModelBase
{
    public SelfColorGenerationConfigurationViewModel(LightingZone zone,
        ParameterViewModelFactory parameterViewModelFactory)
    {
        _zone = zone;
        _parameterViewModelFactory = parameterViewModelFactory;
        
        Init();
    }

    
    private readonly ParameterViewModelFactory _parameterViewModelFactory;
    private readonly LightingZone _zone;

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