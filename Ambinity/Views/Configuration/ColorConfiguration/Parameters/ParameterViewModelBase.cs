using System;
using Ambinity.ViewModels;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public abstract class ParameterViewModelBase : ViewModelBase
{
    public ParameterViewModelBase()
    {
            
    }
    /// <summary>
    /// raise When user finish editting this parameter
    /// </summary>
    public event Action ValueChanged;
    /// <summary>
    /// Header to display
    /// </summary>
    public string Header { get; set; }
    /// <summary>
    /// Description to display
    /// </summary>
    public string Description { get; set; }

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
}