using System.Windows.Input;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public class SelfColorGenerationConfigurationViewModel : ColorConfigurationViewModelBase
{
    public SelfColorGenerationConfigurationViewModel(SelfGeneratedColorConfiguration configuration,
        ParameterViewModelFactory parameterViewModelFactory)
    {
        _parameterViewModelFactory = parameterViewModelFactory;
        _configuration = configuration;
        Init();
    }


    private SelfGeneratedColorConfiguration _configuration;
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