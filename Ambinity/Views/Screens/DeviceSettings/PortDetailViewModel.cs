using System.Collections.ObjectModel;
using Ambinity.ViewModels;

namespace Ambinity.Views.Screens.DeviceSettings;

public class PortDetailViewModel : ViewModelBase
{
    public PortDetailViewModel(AmbinityDeviceViewModelFactory deviceViewModelFactory)
    {
        _deviceViewModelFactory = deviceViewModelFactory;
    }

    public void Init(DevicePortViewModel port)
    {
        CurrentPort = port;
        OnPropertyChanged(nameof(Name));
        DetailViewModel = _deviceViewModelFactory.GetDetailViewModel(port.Output.Device);

    }

    private DevicePortViewModel _currentPort;

    public DevicePortViewModel CurrentPort
    {
        get => _currentPort;
        set
        {
            _currentPort = value;
            OnPropertyChanged();
        }
    }

    private AmbinityDeviceDetailViewModel _detailViewModel;
    private readonly AmbinityDeviceViewModelFactory _deviceViewModelFactory;

    public AmbinityDeviceDetailViewModel DetailViewModel
    {
        get => _detailViewModel;
        set
        {
            _detailViewModel = value;
            OnPropertyChanged();
        }
    }

    public string Name => "Chanel " + (CurrentPort.Output.Index +1).ToString();
}