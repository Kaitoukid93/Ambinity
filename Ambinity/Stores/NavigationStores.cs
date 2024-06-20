using System;
using Ambinity.ViewModels;

namespace Ambinity.Stores;

public class NavigationStores
{
    public event Action CurrentViewModelChanged;
    private ViewModelBase _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get
        {
            return _currentViewModel;
        }
        set
        {
            _currentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}