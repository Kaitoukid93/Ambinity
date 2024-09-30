using System.Collections.Generic;
using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using Ambinity.Views.Screens.DeviceSettings;
using AmbinityCore.Models.Device;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DevicePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public DevicePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel,
        PositionConfigurationViewModel positionConfigurationViewModel,
        AmbinityDeviceViewModelFactory deviceViewModelFactory)
    {
        _deviceViewModelFactory = deviceViewModelFactory;
        PositionConfiguration = positionConfigurationViewModel;
        _canvasViewModel = canvasViewModel;
        Init();
    }

    public override void UpdateObjectProperties()
    {
        var selectedItems = _canvasViewModel.Canvas.Selection.All;
        if (selectedItems.Count == 0)
        {
            DisableEdit();
            Header = NullHeader();
            DetailViewModel = null;
            //clear view
        }
        else if (selectedItems.Count == 1)
        {
            var fig = selectedItems.First();
            fig.PositionPropertyChanged += OnItemPositionChanged;
            var device = (fig as DeviceContainerFigure)?.ChildItem as AmbinityDevice;
            if (device == null || !fig.IsDragable)
            {
                DisableEdit();
                return;
            }

            _selectedDevice = device;
            EnableEdit();
            PositionConfiguration.Init(device);


            Header = GetHeader(device);
            DetailViewModel = _deviceViewModelFactory.GetDetailViewModel(_selectedDevice);
        }
        else
        {
            Header = MultipleSelectedHeader(selectedItems.Count);
            _selectedDevices?.Clear();
            foreach (var item in selectedItems)
            {
                var device = (item as DeviceContainerFigure)?.ChildItem as AmbinityDevice;
                _selectedDevices.Add(device);
            }

            DetailViewModel = _deviceViewModelFactory.GetMultipleDetailViewModel(_selectedDevices.Count);
            DisableEdit();
        }
    }

    private List<AmbinityDevice> _selectedDevices = new List<AmbinityDevice>();
    private AmbinityDevice _selectedDevice;

    private void OnItemPositionChanged(float arg1, float arg2)
    {
        PositionConfiguration.Update();
    }


    private Draw2DCanvasViewModel _canvasViewModel;
    private PositionConfigurationViewModel _positionConfiguration;
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

    public override void DisableEdit()
    {
        PositionConfiguration.IsEnabled = false;
    }

    public override void EnableEdit()
    {
        PositionConfiguration.IsEnabled = true;
    }

    public PositionConfigurationViewModel PositionConfiguration
    {
        get => _positionConfiguration;
        set
        {
            _positionConfiguration = value;
            OnPropertyChanged();
        }
    }

    private ConfigurationHeaderViewModel _header;

    public ConfigurationHeaderViewModel Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged();
        }
    }

    private ConfigurationHeaderViewModel GetHeader(AmbinityDevice device)
    {
        var header = new ConfigurationHeaderViewModel(null, "null");

        header.Icon = "slaveDevice";
        header.Header = device.DeviceName;

        return header;
    }

    private ConfigurationHeaderViewModel NullHeader()
    {
        return new ConfigurationHeaderViewModel("Select an item to begin", "void_selected");
    }

    private ConfigurationHeaderViewModel MultipleSelectedHeader(int count)
    {
        return new ConfigurationHeaderViewModel(count + " " + "items selected", "Zones");
    }
}