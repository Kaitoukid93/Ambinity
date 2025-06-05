using System.Collections.Generic;
using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.Canvas;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using Ambinity.Views.Screens.DeviceSettings;
using AmbinityCore.Models.Device;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DevicePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public DevicePropertiesViewModel(CanvasViewModelFactory  canvasViewModelFactory,
        PositionConfigurationViewModel positionConfigurationViewModel,
        AmbinityDeviceViewModelFactory deviceViewModelFactory, ConfigurationHeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        _deviceViewModelFactory = deviceViewModelFactory;
        PositionConfiguration = positionConfigurationViewModel;
        _canvasViewModel = canvasViewModelFactory.Get<DeviceLayoutCanvasViewModel>();
    }

    public override void UpdateObjectProperties()
    {
        if (_canvasViewModel.IsLocked)
            return;
        var selectedItems = _canvasViewModel.Canvas.Selection.All;
        _headerViewModel.Init(selectedItems);
        OnPropertyChanged(nameof(Header));
        if (selectedItems.Count == 0)
        {
            //clear view
            DisableEdit();
            DetailViewModel = null;
        }
        else if (selectedItems.Count == 1)
        {
            var fig = selectedItems.First();
            fig.PositionPropertyChanged += OnItemPositionChanged;
            var device = (fig as DeviceContainerFigure)?.ChildItem as AmbinityDevice;
            if (device == null)
                return;
            EnableEdit();
            if (!fig.IsDragable)
            {
                EnablePositionEdit = false;
            }
            _selectedDevice = device;
            PositionConfiguration.Init(device);
            DetailViewModel = _deviceViewModelFactory.GetDetailViewModel(_selectedDevice);
        }
        else
        {
            _selectedDevices?.Clear();
            foreach (var device in selectedItems.Where(i => i.IsDragable).Select(item =>
                         (item as DeviceContainerFigure)?.ChildItem as AmbinityDevice))
            {
                _selectedDevices.Add(device);
            }

            if (_selectedDevices.Count == 0)
                return;
            DetailViewModel = _deviceViewModelFactory.GetMultipleDetailViewModel(_selectedDevices.Count);
            DisableEdit();
        }
    }

    private List<AmbinityDevice> _selectedDevices = [];
    private AmbinityDevice _selectedDevice;

    private void OnItemPositionChanged(float arg1, float arg2)
    {
        if (EnablePositionEdit)
            PositionConfiguration.Update();
    }

    private ConfigurationHeaderViewModel _headerViewModel;
    private readonly DeviceLayoutCanvasViewModel _canvasViewModel;
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

    public PositionConfigurationViewModel PositionConfiguration
    {
        get => _positionConfiguration;
        set
        {
            _positionConfiguration = value;
            OnPropertyChanged();
        }
    }

    public ConfigurationHeaderViewModel Header => _headerViewModel;

    public override void DisableEdit()
    {
        EnablePositionEdit = false;
        DetailViewModel?.Dispose();
    }

    public override void EnableEdit()
    {
        EnablePositionEdit = true;
    }

    private bool _enablePositionEdit;

    public bool EnablePositionEdit
    {
        get => _enablePositionEdit;
        set
        {
            _enablePositionEdit = value;
            OnPropertyChanged();
        }
    }
}
