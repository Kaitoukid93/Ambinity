using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZonePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public ZonePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel,
        ColorConfigurationViewModelFactory colorConfigurationViewModelFactory,
        PositionConfigurationViewModel positionConfigurationViewModel)
    {
        PositionConfiguration = positionConfigurationViewModel;
        _canvasViewModel = canvasViewModel;
        _colorConfigurationViewModelFactory = colorConfigurationViewModelFactory;
        Init();
    }

    private Figure _selectedItem;

    public override void UpdateObjectProperties()
    {
        var selectedItems = _canvasViewModel.Canvas.Selection.All;
        if (selectedItems.Count == 0)
        {
            _zone = null;
            DisableEdit();
            Header = NullHeader();
           _selectedItem = null;
            //clear view
        }
        else if (selectedItems.Count == 1)
        {
            var fig = selectedItems.First();

            //selection filter to prevent race condition load
            if (_selectedItem == null)
                _selectedItem = fig;
            else
            {
                if (_selectedItem == fig)
                    return;
                else
                {
                    _selectedItem = fig;
                }
            }

            if (fig is not ContainerFigure)
                return;
            var zone = (fig as LightingZoneFigure)?.ChildItem as LightingZone;
            if (_zone == zone)
                return;
            fig.PositionPropertyChanged += OnItemPositionChanged;

            if (zone == null)
                return;
            if (!fig.IsResizable)
            {
                PositionConfiguration.IsEnabled = false;
            }

            else
            {
                EnableEdit();
                PositionConfiguration.Init(zone);
            }

            _zone = zone;
            //show full view
            ColorConfiguration?.Dispose();
            ColorConfiguration = _colorConfigurationViewModelFactory.GetColorConfiguration(zone.LightingConfiguration);
            Header = GetHeader(zone.LightingConfiguration);
        }
        else
        {
            _zone = null;
            _selectedItem = null;
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
    private ColorConfigurationViewModelFactory _colorConfigurationViewModelFactory;

    public override void DisableEdit()
    {
        PositionConfiguration.IsEnabled = false;
        ColorConfiguration?.Dispose();
        ColorConfiguration = new NullColorConfigurationViewModel();
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

    private ConfigurationHeaderViewModel GetHeader(ILightingConfiguration config)
    {
        var header = new ConfigurationHeaderViewModel(_zone.Shape.ToString() + " - " + _zone.LightingConfiguration.Name,
            _zone.Icon);
        return header;
    }

    private ConfigurationHeaderViewModel NullHeader()
    {
        return new ConfigurationHeaderViewModel("Please select an item to begin", "void_selected", true);
    }

    private ConfigurationHeaderViewModel MultipleSelectedHeader(int count)
    {
        return new ConfigurationHeaderViewModel(count + " " + "items selected", "Zones");
    }
}