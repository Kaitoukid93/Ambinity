using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Screens.DeviceLayout.Library;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using DynamicData;

namespace Ambinity.Views.Screens.DeviceSettings;

public class PortDetailViewModel : ViewModelBase
{
    public ICommand AddNewDaisyChainDeviceCommand { get; }
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;
    public bool IsMultiplePortsSelected => _selectedPorts.Count > 1;
    private AmbinityDeviceDaisyChainElementViewModel _selectedDevice;

    public PortDetailViewModel(AmbinityDeviceViewModelFactory deviceViewModelFactory,
        DeviceLayoutsLibraryViewModel layoutsLibraryViewModel)
    {
        _layoutsLibraryViewModel = layoutsLibraryViewModel;
        _deviceViewModelFactory = deviceViewModelFactory;
        AddNewDaisyChainDeviceCommand = new RelayCommand(AddNewDaisyChainDevice);
    }


    private void AddNewDaisyChainDevice()
    {
        //if chain contains any device, take last device and clone then add to bottom of the chain
        //at the moment the chain will never zero count, because the output always being init with default device, see SerialControllerProvider
        //or OpenRGBControllerProvider for further details
        //only single port selection can daisychain
        if (_selectedPorts.Count > 1)
            return;
        //simply add new device to output devices and re-init the viewmodel
        var selectedPort = _selectedPorts.First();
        if (selectedPort != null)
        {
            var bottomDevice = (Devices.Last() as AmbinityDeviceDaisyChainElementViewModel).Device;
            var newDevice = new AmbinityDevice(bottomDevice.Layout);
            newDevice.LoadLayout();
            selectedPort.Output.AddDeviceToOutputChain(newDevice);
        }

        //add device to reposiitory
        //reload view 
        LoadDevices();
        //create new device with the layout
        //if chain contains no device, take default device, not using for now
    }

    public FlyoutContentViewModelBase FlyoutViewModel { get; set; }


    private void OnLibraryItemSelected(AssetItemViewModelBase item)
    {
        if (item is AmbinityDeviceLayoutAssetViewModel layoutAsset)
        {
            var layout = layoutAsset.Item as AmbinityDeviceLayout;
            foreach (var port in _selectedPorts)
            {
                foreach (var device in port.Output.Devices)
                {
                    device.LoadLayout(layout);
                }
            }
        }

        //apply layout
    }

    public void OpenFlyout(FlyoutContentViewModelBase flyoutViewModel)
    {
        FlyoutViewModel = flyoutViewModel;
        OpenFlyoutEvent?.Invoke();
    }

    public void OnFlyoutClosing()
    {
        FlyoutViewModel.Dispose();
        FlyoutViewModel = null;
    }

    public void Init(DevicePortViewModel port)
    {
        _selectedPorts =
        [
            port
        ];
        Name = "Chanel " + (port.Output.Index + 1).ToString();
        Brightness = port.Output.Brightness;
        IsEnabled = port.Output.IsEnabled;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(IsMultiplePortsSelected));
        LoadDevices();
    }

    private void LoadDevices()
    {
        if (_selectedPorts.Count != 1)
            return;

        Devices?.Clear();
        for (int i = 0; i < _selectedPorts[0].Output.Devices.Count; i++)
        {
            var deviceElement =
                _deviceViewModelFactory.GetDeviceDaisyChainElementViewModel(_selectedPorts[0].Output.Devices[i]);
            deviceElement.Selected += OnDeviceSelected;
            deviceElement.Detach += OnDeviceDetach;
            deviceElement.ChangeDevice += OnChangeDeviceRequest;
            Devices.Add(deviceElement);
            if (i < _selectedPorts[0].Output.Devices.Count - 1)
            {
                Devices.Add(new DaisyChainPlusSymbolViewModel());
            }
        }
    }

    private void OnDeviceDetach(AmbinityDeviceDaisyChainElementViewModel device)
    {
        if (_selectedPorts.Count > 1)
            return;
        //simply add new device to output devices and re-init the viewmodel
        var selectedPort = _selectedPorts.First();
        if (selectedPort != null)
        {
            selectedPort.Output.RemoveDeviceFromOutputChain(device.Device);
        }

        LoadDevices();
    }

    private void OnChangeDeviceRequest(AmbinityDeviceDaisyChainElementViewModel obj)
    {
        _layoutsLibraryViewModel?.Init();
        _layoutsLibraryViewModel.ItemSelected += OnLibraryItemSelected;
        OpenFlyout(_layoutsLibraryViewModel);
    }

    private void OnDeviceSelected(AmbinityDeviceDaisyChainElementViewModel device)
    {
        _selectedDevice = device;
    }

    public void Init(List<DevicePortViewModel> ports)
    {
        _selectedPorts = ports;
        Name = ports.Count.ToString() + " Chanel selected ";
        OnPropertyChanged(nameof(Name));
        //port will be reenabled when select multiple
        IsEnabled = true;
        //brightness will be default
        Brightness = 153;
        OnPropertyChanged(nameof(IsMultiplePortsSelected));
        // DetailViewModel = _deviceViewModelFactory.GetMultipleDetailViewModel(_selectedPorts.Count);
    }

    private List<DevicePortViewModel> _selectedPorts;

    private ObservableCollection<DaisyChainItemViewModelBase> _devices =
        [];

    private readonly AmbinityDeviceViewModelFactory _deviceViewModelFactory;

    public ObservableCollection<DaisyChainItemViewModelBase> Devices
    {
        get => _devices;
        set
        {
            _devices = value;
            OnPropertyChanged();
        }
    }

    public string Name { get; set; }

    private int _brightness;

    //todo implement port identify by toggling all led attached to the port
    public int Brightness
    {
        get => _brightness;
        set
        {
            _brightness = value;
            foreach (var port in _selectedPorts)
            {
                port.Output.SetBrightness(value);
            }

            OnPropertyChanged();
        }
    }

    private bool _isEnabled;
    private readonly DeviceLayoutsLibraryViewModel _layoutsLibraryViewModel;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            foreach (var port in _selectedPorts)
            {
                port.Output.IsEnabled = value;
            }

            OnPropertyChanged();
        }
    }


    public override void Dispose()
    {
        _layoutsLibraryViewModel.ItemSelected -= OnLibraryItemSelected;
    }
}