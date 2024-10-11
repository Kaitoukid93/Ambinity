using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class DevicesPageViewModel : ViewModelBase
{
    private readonly SerialControllerRepository _serialControllerRepository;
    private readonly OpenRGBControllerRepository _openRGBControllerRepository;
    private readonly QuickAccessViewModelFactory _factory;


    public DevicesPageViewModel(SerialControllerRepository serialControllerRepository, OpenRGBControllerRepository openRGBControllerRepository, QuickAccessViewModelFactory factory)
    {
        _factory = factory;
        _serialControllerRepository = serialControllerRepository;
         _openRGBControllerRepository = openRGBControllerRepository;
         _serialControllerRepository.ItemAdded += OnItemAdded;
         Devices = new ObservableCollection<QuickAccessDeviceViewModel>();
    }

    public void Init()
    {
        foreach (var controler in _serialControllerRepository.Items)
        {
            OnItemAdded(controler as SerialController);
        }
        
        foreach (var controler in _openRGBControllerRepository.Items)
        {
            OnItemAdded(controler as OpenRGBController);
        }
    }

    private void OnItemAdded(ICollectableItem item)
    {
        var vm = _factory.GetDeviceViewModel(item as IController);
        Devices.Add(vm);
    }
    public ObservableCollection<QuickAccessDeviceViewModel> Devices { get; }
}