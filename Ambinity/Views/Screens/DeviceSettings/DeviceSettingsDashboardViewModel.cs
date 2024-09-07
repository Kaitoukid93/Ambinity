using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceSettingsDashboardViewModel : ViewModelBase
{
    public DeviceSettingsDashboardViewModel(RootNavigationStores rootNavigationStores,
        SerialControllerRepository serialControllerRepository, OpenRGBControllerRepository openRgbControllerRepository, DeviceSettingsInfoBarViewModel infoBarViewModel)
    {
        _rootNavigationStores = rootNavigationStores;
        InfoBarViewModel = infoBarViewModel;
        _serialControllerRepository = serialControllerRepository;
        _openRgbControllerRepository = openRgbControllerRepository;
    }

    private void OnNewControllerAdded(IController controller)
    {
        AddController(controller);
    }

    public DeviceSettingsInfoBarViewModel InfoBarViewModel { get; set; }
    private SerialControllerRepository _serialControllerRepository;
    private OpenRGBControllerRepository _openRgbControllerRepository;

    private readonly RootNavigationStores _rootNavigationStores;

    private void OnDeviceClicked(DashboardDeviceViewModel device)
    {
        GotoDeviceControlCommand.Execute(device);
    }

    public ObservableCollection<DashboardDeviceViewModel> Devices { get; set; }

    public void Init()
    {
        //load available devices
        //setup commands
        _serialControllerRepository.NewControllerAdded += OnNewControllerAdded;
        _serialControllerRepository.OldDeviceReconnected += OnOldControllerReconnected;
        _openRgbControllerRepository.NewControllerAdded += OnNewControllerAdded;
        _openRgbControllerRepository.OldDeviceReconnected += OnOldControllerReconnected;
        Devices = new ObservableCollection<DashboardDeviceViewModel>();
        foreach (var item in _serialControllerRepository.Items)
        {
            AddController((item as SerialController));
        }

        foreach (var item in _openRgbControllerRepository.Items)
        {
            AddController((item as OpenRGBController));
        }
        CommandSetup();
    }

    private void OnOldControllerReconnected(IController obj)
    {
        InfoBarViewModel.IsOpen = false;
    }

    public void AddController(IController controller)
    {
        var deviceVm = new DashboardDeviceViewModel(controller);
        deviceVm.DeviceClicked += GoToDeviceControl;
        Devices.Add(deviceVm);
        InfoBarViewModel.IsOpen = false;
    }

    private void CommandSetup()
    {
        GotoDeviceControlCommand = new RelayCommand<DashboardDeviceViewModel>(GoToDeviceControl);
    }

    private void GoToDeviceControl(DashboardDeviceViewModel device)
    {
        var vm = Ioc.Default.GetRequiredService<DeviceSettingsViewModel>();
        vm.Init(device.Controller);
        _rootNavigationStores.CurrentViewModel = vm;
    }

    private bool _isInfoBarOpen;

    public bool IsInforBarOpen
    {
        get => _isInfoBarOpen;
        set
        {
            _isInfoBarOpen = value;
            OnPropertyChanged();
        }
    }

    public ICommand GotoDeviceControlCommand { get; set; }
}