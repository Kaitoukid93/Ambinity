using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Avalonia.Threading;

namespace Ambinity.Views.Configuration.ColorConfiguration;

public abstract class ColorConfigurationViewModelBase : ViewModelBase
{
    public ColorConfigurationViewModelBase()
    {
        Parameters = new ObservableCollection<ParameterViewModelBase>();
    }
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }
    public void Reset()
    {
        
    }
    private ObservableCollection<ParameterViewModelBase> _parameters;

    public ObservableCollection<ParameterViewModelBase> Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            OnPropertyChanged();
        }
    }
}