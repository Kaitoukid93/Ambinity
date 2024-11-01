using System;
using Ambinity.ViewModels;

namespace Ambinity.Views.AmbinityStore;

public class AmbinityStoreNavigation
{
    public event Action<ViewModelBase> CurrentViewModelChanged;
    private ViewModelBase _currentViewModel;
    public event Action GoBackRequested;

    public ViewModelBase CurrentViewModel
    {
        get { return _currentViewModel; }
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