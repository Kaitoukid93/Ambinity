using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using Ambinity.Views.Screens.DeviceLayout.Library;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.Screens.DeviceSettings;

public class PortDetailViewModel : ViewModelBase
{
    public ICommand OpenLibraryCommand { get; set; }
    public event Action OpenFlyoutEvent;
    public event Action CloseFlyoutEvent;
    public PortDetailViewModel(AmbinityDeviceViewModelFactory deviceViewModelFactory, DeviceLayoutsLibraryViewModel layoutsLibraryViewModel)
    {
        _layoutsLibraryViewModel = layoutsLibraryViewModel;
        _deviceViewModelFactory = deviceViewModelFactory;
        OpenLibraryCommand = new RelayCommand(OpenDeviceLibrary);
    }
    public FlyoutContentViewModelBase FlyoutViewModel { get; set; }
    private void OpenDeviceLibrary()
    {
        _layoutsLibraryViewModel?.Init();
        _layoutsLibraryViewModel.ItemSelected += OnLibraryItemSelected;
        OpenFlyout(_layoutsLibraryViewModel);
    }

    private void OnLibraryItemSelected(ICollectableItem item)
    {
        foreach (var port in _selectedPorts)
        {
            var device = port.Output.Device;
            device.LoadLayout(item as AmbinityDeviceLayout);
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
        _selectedPorts = new List<DevicePortViewModel>();
        _selectedPorts.Add(port);
        Name = "Chanel " + (port.Output.Index + 1).ToString();
        Brightness = port.Output.Brightness;
        IsEnabled = port.Output.IsEnabled;
        OnPropertyChanged(nameof(Name));
        DetailViewModel = _deviceViewModelFactory.GetDetailViewModel(port.Output.Device);
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
        DetailViewModel = _deviceViewModelFactory.GetMultipleDetailViewModel(_selectedPorts.Count);
    }

    private List<DevicePortViewModel> _selectedPorts;
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

    public string Name { get; set; }

    private int _brightness;

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