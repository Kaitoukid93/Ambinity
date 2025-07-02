using System;
using System.ComponentModel;
using System.Windows.Input;
using Ambinity.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class LEDLayoutCreatorInitialViewModel : ViewModelBase
{

    public event Action Cancel;
    public event Action<LayoutPropertiesViewModel> Accept;

    public LEDLayoutCreatorInitialViewModel()
    {
        LayoutPropertiesViewModel = new LayoutPropertiesViewModel();
        LayoutPropertiesViewModel.Init();
        AcceptCommand = new RelayCommand(OnUserAcceptNewLayout);
        CancelCommand = new RelayCommand(OnUserCancel);
    }

    private void OnUserCancel()
    {
        Cancel?.Invoke();
    }

    private void OnUserAcceptNewLayout()
    {
        Accept?.Invoke(LayoutPropertiesViewModel);
    }

    public LayoutPropertiesViewModel LayoutPropertiesViewModel { get; set; }
    public RelayCommand AcceptCommand { get; }
    public ICommand CancelCommand { get; }
}
