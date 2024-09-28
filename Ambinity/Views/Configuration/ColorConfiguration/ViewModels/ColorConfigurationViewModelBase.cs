
using System.Collections.ObjectModel;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;


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
    public virtual void Reset()
    {
        
    }

    public virtual void Init()
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

    public override void Dispose()
    {
        base.Dispose();
        foreach (var param in Parameters)
        {
            param.Dispose();
        }
    }
}