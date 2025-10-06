using System;
using System.Collections.Generic;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device.Device;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;


namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class SaveLayoutDialogViewModel : ViewModelBase
{

    public event Action Cancel;
    public event Action Accept;

    public SaveLayoutDialogViewModel(List<Figure> leds)
    {
        LEDsCount = leds.Count;
        AcceptCommand = new RelayCommand(OnUserAccept);
        CancelCommand = new RelayCommand(OnUserCancel);
    }
    public string LayoutName { get; set; }
    public string LayoutDescription { get; set; }
    public int LEDsCount { get; set; }
    public DeviceLayoutType LayoutType { get; set; }

    private void OnUserCancel()
    {
        Cancel?.Invoke();
    }

    private void OnUserAccept()
    {
        Accept?.Invoke();
    }


    public RelayCommand AcceptCommand { get; }
    public ICommand CancelCommand { get; }

}
