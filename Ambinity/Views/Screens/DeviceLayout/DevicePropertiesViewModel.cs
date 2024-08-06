using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;
using AmbinityCore.Models.Device;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;

namespace Ambinity.Views.Screens.DeviceLayout;

public class DevicePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public DevicePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel)
    {
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
            var device = (fig as DeviceContainerFigure).ChildItem as AmbinityDevice;
            if (device == null)
                DisableEdit();
            else
            {
                PositionConfiguration = new PositionConfigurationViewModel(_canvasViewModel);
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

    private LightingZone _zone;
    private Draw2DCanvasViewModel _canvasViewModel;
    private PositionConfigurationViewModel _positionConfiguration;

    public override void DisableEdit()
    {
        PositionConfiguration = new PositionConfigurationViewModel(_canvasViewModel);
        PositionConfiguration.IsEnabled = false;
        ColorConfiguration = new NullColorConfigurationViewModel();
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

    private ColorConfigurationViewModelBase _colorConfiguration;

    public ColorConfigurationViewModelBase ColorConfiguration
    {
        get => _colorConfiguration;
        set
        {
            _colorConfiguration = value;
            OnPropertyChanged();
        }
    }

    private PropertiesViewHeader _header;

    public PropertiesViewHeader Header
    {
        get => _header;
        set
        {
            _header = value;
            OnPropertyChanged();
        }
    }

    private PropertiesViewHeader GetHeader(AmbinityDevice device)
    {
        var header = new PropertiesViewHeader(null, null);

        header.Icon = "slaveDevice";
        header.Header = device.DeviceName;

        return header;
    }

    private PropertiesViewHeader NullHeader()
    {
        return new PropertiesViewHeader("0 item selected", null, false);
    }

    private PropertiesViewHeader MultipleSelectedHeader(int count)
    {
        return new PropertiesViewHeader(count + " " + "items selected", "Zones");
    }
}