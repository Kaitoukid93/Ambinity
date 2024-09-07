using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using AmbinityCore.Models.Device;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DevicePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public DevicePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel, PositionConfigurationViewModel positionConfigurationViewModel)
    {
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
            //clear view
        }
        else if (selectedItems.Count == 1)
        {
            var fig = selectedItems.First();
            fig.PositionPropertyChanged += OnItemPositionChanged;
            var device = (fig as DeviceContainerFigure)?.ChildItem as AmbinityDevice;
            if (device == null|| !fig.IsDragable)
            {
                DisableEdit();
                return;
            }
            else
            {
                _device = device;
                EnableEdit();
                PositionConfiguration.Init(device);
            }
            Header = GetHeader(device);
        }
        else
        {
            Header = MultipleSelectedHeader(selectedItems.Count);
            DisableEdit();
        }
    }


    private void OnItemPositionChanged(float arg1, float arg2)
    {
        PositionConfiguration.Update();
    }

    private AmbinityDevice _device;
    private Draw2DCanvasViewModel _canvasViewModel;
    private PositionConfigurationViewModel _positionConfiguration;

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