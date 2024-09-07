using System;
using System.Windows.Input;
using Ambinity.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.LayoutEditor;

public abstract class FlyoutContentViewModelBase : ViewModelBase
{
    public virtual event Action? Close;

    public FlyoutContentViewModelBase()
    {
        CloseCommand = new RelayCommand(CloseFlyout);
    }
    private void CloseFlyout()
    {
        Close?.Invoke();
    }

    public virtual void Dispose()
    {
        
    }
    public ICommand CloseCommand { get; set; }
}