using System;
using System.Collections.Generic;
using Ambinity.ViewModels;
using Ambinity.Views.Screens.DeviceSettings;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Device.Service;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Windows;

public class NewDeviceDialogContentViewModel : ViewModelBase
{
    private ContentDialog _dialog;
    public EventHandler? DialogClosed;
    public List<ControllerPortViewModel> AvailablePorts { get; set; }

    private ControllerPortViewModel _selectedPort;
    public ControllerPortViewModel SelectedPort
    {
        get => _selectedPort;
        set
        {
            _selectedPort = value;
            _dialog.IsPrimaryButtonEnabled = _selectedPort != null;
            OnPropertyChanged(nameof(SelectedPort));
        }
    }
    private DeviceCardViewModel _selectedDeviceCard;
    public DeviceCardViewModel SelectedDeviceCard
    {
        get => _selectedDeviceCard;
        set
        {
            _selectedDeviceCard = value;
            OnPropertyChanged(nameof(SelectedDeviceCard));
        }
    }
    private List<DeviceCardViewModel> _deviceCards;
    public List<DeviceCardViewModel> DeviceCards
    {
        get => _deviceCards;
        set
        {
            _deviceCards = value;
            OnPropertyChanged(nameof(DeviceCards));
        }
    }

    public RelayCommand RefreshPortsCommand { get; }

    public NewDeviceDialogContentViewModel()
    {
        DeviceCards = [];
        AvailablePorts = [];
        RefreshPortsCommand = new RelayCommand(RefreshPorts);

    }

    private void RefreshPorts()
    {
        AvailablePorts?.Clear();
        var knownDevices = new List<(string vid, string pid)>
        {
            ("1209", "c550"), // CH55X
            ("1A86", "7522"), // CH340
            ("239A", "CAFE")  // Ada
        };

        foreach (var (vid, pid) in knownDevices)
        {
            var foundPorts = SerialControllerEnumerator.GetSerialPortByID(vid, pid);
            foreach (var port in foundPorts)
            {
                if (AvailablePorts.Exists(p => p.PortAddres == port))
                {
                    continue; // Skip if port already exists
                }
                var portInfo = new ControllerPortViewModel(port, vid, pid);
                AvailablePorts.Add(portInfo);
            }
        }
    }

    public void Init(ContentDialog dialog)
    {
        if (dialog is null)
        {
            throw new ArgumentNullException(nameof(dialog));
        }
        _dialog = dialog;
        _dialog.IsPrimaryButtonEnabled = _selectedPort != null;
        dialog.Closed += DialogOnClosed;
        CreateDefaultAmbinoDevices();
        RefreshPorts();
        SelectedDeviceCard = DeviceCards.Count > 0 ? DeviceCards[0] : null;
    }


    private void CreateDefaultAmbinoDevices()
    {
        var ambinoBasic = new SerialController()
        {
            Name = "Ambino Basic",
            Description = "Ambino Basic with default settings",
            SerialPort = "COM1",
            HardwareType = HardwareTypeEnum.AmbinoBasic
        };
        var ambinoFanHub = new SerialController()
        {
            Name = "Ambino FanHub",
            Description = "Ambino Fan Hub with default settings",
            SerialPort = "COM1",
            HardwareType = HardwareTypeEnum.AmbinoFanHub
        };
        var ambinoHUBV3 = new SerialController()
        {
            Name = "Ambino HubV3",
            Description = "Ambino HUB V3 with default settings",
            SerialPort = "COM1",
            HardwareType = HardwareTypeEnum.AmbinoHUBV3
        };
        var ambinoEdge = new SerialController()
        {
            Name = "Ambino Edge",
            Description = "Ambino Edge with default settings",
            SerialPort = "COM1",
            HardwareType = HardwareTypeEnum.AmbinoEDGE
        };
        DeviceCards = new List<DeviceCardViewModel>
        {
            new DeviceCardViewModel(ambinoBasic),
            new DeviceCardViewModel(ambinoFanHub),
            new DeviceCardViewModel(ambinoHUBV3),
            new DeviceCardViewModel(ambinoEdge)
    };
    }

    private void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _dialog.Closed -= DialogOnClosed;
        DialogClosed?.Invoke(this, args);
    }

    public class ControllerPortViewModel : ViewModelBase
    {
        public string PortAddres { get; set; }
        public string PID { get; set; }
        public string VID { get; set; }

        public ControllerPortViewModel(string portAddress, string vid, string pid)
        {
            PortAddres = portAddress;
            VID = vid;
            PID = pid;
        }

    }
}
