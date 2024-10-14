using System;
using Ambinity.ViewModels;

namespace Ambinity.Stores;

public class QuickAccessNavigationStore
{
    public event Action<ViewModelBase> CurrentViewModelChanged;
    public event Action GoBackRequested;
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

    public void GoBack()
    {
        GoBackRequested?.Invoke();
    }
}