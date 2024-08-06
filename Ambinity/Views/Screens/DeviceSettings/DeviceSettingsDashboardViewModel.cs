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
    public DeviceSettingsDashboardViewModel(RootNavigationStores rootNavigationStores,SerialControllerRepository repository, DeviceSettingsInfoBarViewModel infoBarViewModel)
    {
        _rootNavigationStores = rootNavigationStores;
        InfoBarViewModel = infoBarViewModel;
        _repository = repository;
        
    }

    private void OnNewControllerAdded(SerialController controller)
    {
        
        AddController(controller);
    }
    public DeviceSettingsInfoBarViewModel InfoBarViewModel { get; set; }
    private SerialControllerRepository _repository;

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
        _repository.NewControllerAdded += OnNewControllerAdded;
        _repository.OldDeviceReconnected += OnOldControllerReconnected;
        Devices = new ObservableCollection<DashboardDeviceViewModel>();
        foreach (var item in _repository.Items)
        {
            AddController((item as SerialController));
        }

        CommandSetup();
    }

    private void OnOldControllerReconnected(SerialController obj)
    {
        InfoBarViewModel.IsOpen = false;
    }

    public void AddController(SerialController controller)
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