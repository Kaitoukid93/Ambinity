using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;

namespace Ambinity.Views.Screens.DeviceSettings;

public class DeviceSettingsDashboardViewModel : ScreenViewModelBase
{
    public DeviceSettingsDashboardViewModel(RootNavigationStores rootNavigationStores, SerialControllerDiscoveryService discoveryService,
        SerialControllerRepository serialControllerRepository, OpenRGBControllerRepository openRgbControllerRepository,
        DeviceSettingsInfoBarViewModel infoBarViewModel, IDialogService dialogService, DeviceSettingsViewModel deviceSettingsViewModel, ThumbnailService thumbnailService, LightingProfileDecoder decoder)
    {
        _discoveryService = discoveryService;
        _decoder = decoder;
        _thumbnailService = thumbnailService;
        _deviceSettingsViewModel = deviceSettingsViewModel;
        _dialogService = dialogService;
        _rootNavigationStores = rootNavigationStores;
        InfoBarViewModel = infoBarViewModel;
        _serialControllerRepository = serialControllerRepository;
        _openRgbControllerRepository = openRgbControllerRepository;
        AddNewDeviceCommand = new AsyncRelayCommand(AddNewDevice);
    }

    private async Task AddNewDevice()
    {
        var vm = new NewDeviceDialogContentViewModel();
        vm.DialogClosed += OnCreateNewDeviceDialogClosed;
        await _dialogService.ShowCreateNewDeviceDialog(vm);

    }

    private void OnCreateNewDeviceDialogClosed(object? sender, EventArgs e)
    {
        var vm = sender as NewDeviceDialogContentViewModel;
        var args = e as ContentDialogClosedEventArgs;
        if (args == null)
            return;
        var result = args.Result;
        if (result == ContentDialogResult.Secondary || result == ContentDialogResult.None)
            return;
        if (result == ContentDialogResult.Primary)
        {
            var selectedDevice = vm.SelectedDeviceCard;
            var selectedPort = vm.SelectedPort;
            var controller = new SerialController
            {
                Name = selectedDevice.Name,
                SerialPort = selectedPort.PortAddres,
                HardwareType = selectedDevice.HardwareType,
                SerialNumber = Guid.NewGuid().ToString(),
            };
            _discoveryService.ManuallyAddController(controller);
        }
    }

    private void OnNewControllerAdded(IController controller)
    {
        AddController(controller);
    }

    public DeviceSettingsInfoBarViewModel InfoBarViewModel { get; set; }
    private SerialControllerRepository _serialControllerRepository;
    private OpenRGBControllerRepository _openRgbControllerRepository;

    public AsyncRelayCommand AddNewDeviceCommand { get; }

    private ThumbnailService _thumbnailService;
    private readonly RootNavigationStores _rootNavigationStores;
    public ObservableCollection<DashboardDeviceViewModel> Devices { get; set; }
    public ObservableCollection<DashboardDeviceViewModel> CoolingDevices { get; set; }

    public override async Task Init()
    {
        //load available devices
        //setup commands
        _serialControllerRepository.NewControllerAdded += OnNewControllerAdded;
        _openRgbControllerRepository.NewControllerAdded += OnNewControllerAdded;
        _serialControllerRepository.ItemRemoved += OnItemRemoved;
        Devices = new ObservableCollection<DashboardDeviceViewModel>();
        OnPropertyChanged(nameof(Devices));
        CoolingDevices = new ObservableCollection<DashboardDeviceViewModel>();
        OnPropertyChanged(nameof(CoolingDevices));
        foreach (var item in _serialControllerRepository.Items)
        {
            AddController((item as SerialController));
        }

        foreach (var item in _openRgbControllerRepository.Items)
        {
            AddController((item as OpenRGBController));
        }

    }

    private void OnItemRemoved(ICollectableItem item)
    {
        var controller = item as IController;
        if (controller == null)
            return;

        var deviceVm = Devices.FirstOrDefault(x => x.Controller == controller);
        if (deviceVm != null)
        {
            Devices.Remove(deviceVm);
            CoolingDevices.Remove(deviceVm);
        }
        InfoBarViewModel.IsOpen = true;
        InfoBarViewModel.IsLoading = false;
        InfoBarViewModel.Title = "Device removed";
        InfoBarViewModel.Content = $"The device {controller.Name} has been removed.";
    }


    public void AddController(IController controller)
    {
        var deviceVm = new DashboardDeviceViewModel(controller, _thumbnailService);
        deviceVm.DeviceClicked += GoToDeviceControl;
        deviceVm.DeviceRemoved += RemoveDevice;
        Devices.Add(deviceVm);
        if (controller.FanController != null)
            CoolingDevices.Add(deviceVm);
        InfoBarViewModel.IsOpen = false;
    }

    private void RemoveDevice(DashboardDeviceViewModel device)
    {
        CollectableItemRepository repo = device.Controller is OpenRGBController
           ? _openRgbControllerRepository
           : _serialControllerRepository;
        repo.RemoveItem(device.Controller);
    }

    private async void GoToDeviceControl(DashboardDeviceViewModel device)
    {
        await _decoder?.Stop();
        var dialogvm = new LoadingDialogViewModel();
        _dialogService.ShowLoadingDialog(dialogvm, "Loading device");
        var result = await Task.Run(() => _deviceSettingsViewModel.Init(device.Controller));
        if (result)
        {
            Log.Error("Failed to load device settings, firmware not supported or not connected.");
        }
        else
        {
            //log out device detail
        }
        dialogvm.Close();
        _rootNavigationStores.CurrentViewModel = _deviceSettingsViewModel;
    }

    private bool _isInfoBarOpen;
    private readonly IDialogService _dialogService;
    private readonly DeviceSettingsViewModel _deviceSettingsViewModel;
    private readonly SerialControllerDiscoveryService _discoveryService;
    private readonly LightingProfileDecoder _decoder;

    public bool IsInforBarOpen
    {
        get => _isInfoBarOpen;
        set
        {
            _isInfoBarOpen = value;
            OnPropertyChanged();
        }
    }
    public override void Dispose()
    {
        base.Dispose();
        _serialControllerRepository.NewControllerAdded -= OnNewControllerAdded;
        _openRgbControllerRepository.NewControllerAdded -= OnNewControllerAdded;
    }
}
