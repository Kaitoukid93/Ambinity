using System;
using Ambinity.ViewModels;

namespace Ambinity.Stores;

public class QuickAccessNavigationStore
{
    public event Action<ViewModelBase> CurrentViewModelChanged;
    private ViewModelBase _currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get
        {
            return _currentViewModel;
        }
        set
        {
            _currentViewModel?.Dispose();
            _currentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }

    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke(CurrentViewModel);
    }
}