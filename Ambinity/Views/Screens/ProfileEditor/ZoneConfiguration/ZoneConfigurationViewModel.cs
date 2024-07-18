using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia;
using Avalonia.Threading;

namespace Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;

/// <summary>
/// Wrapper for list parameters of Lighting zone configuration 
/// </summary>
public class ZoneConfigurationViewModel : ViewModelBase
{
    public ZoneConfigurationViewModel()
    {
        Parameters = new ObservableCollection<ParameterViewModelBase>();
    }

    /// <summary>
    /// Load parameters async
    /// </summary>
    /// <param name="config"></param>
    public async Task Init(ILightingConfiguration config)
    {
        _zoneConfiguration = config;
        //get parameters based on config
        await Task.Run(async () =>
        {
            foreach (var param in ParameterHelper.GetParameters(_zoneConfiguration))
            {
                await AddParameter(param);
            }
        });
    }

    private ILightingConfiguration _zoneConfiguration;
    private ObservableCollection<ParameterViewModelBase> _parameters;

    public ObservableCollection<ParameterViewModelBase> Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            RaisePropertyChanged(nameof(Parameters));
        }
    }

    public async Task AddParameter(ParameterViewModelBase param)
    {
        await Dispatcher.UIThread.InvokeAsync(() => Parameters.Add(param));
        await Task.Delay(200);
    }
}