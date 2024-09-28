using Ambinity.ViewModels;
using AmbinityCore.Models.Device.Controller;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceConnectionSettingsViewModel : ViewModelBase
{
    public DeviceConnectionSettingsViewModel()
    {
        
    }

    public void Init(IController controller)
    {
        Controller = controller;
    }

    public IController Controller { get; set; }
}