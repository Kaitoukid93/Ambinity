using System.Linq;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.LayoutEditor.RightPanel.PropertiesView;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZonePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public ZonePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel,
        ColorConfigurationViewModelFactory colorConfigurationViewModelFactory,
        PositionConfigurationViewModel positionConfigurationViewModel, ConfigurationHeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        PositionConfiguration = positionConfigurationViewModel;
        _canvasViewModel = canvasViewModel;
        _colorConfigurationViewModelFactory = colorConfigurationViewModelFactory;

    }

    private Figure _selectedFigure;

    public override void UpdateObjectProperties()
    {
        var selectedItems = _canvasViewModel.Canvas.Selection.All;
         _headerViewModel.Init(selectedItems);
        OnPropertyChanged(nameof(Header));
        if (selectedItems.Count == 0)
        {
            _zone = null;
            DisableEdit();
            ColorConfiguration?.Dispose();
            _selectedFigure = null;
            //clear view
        }
        else if (selectedItems.Count == 1)
        {
            var fig = selectedItems.First();

            //selection filter to prevent race condition load
            if (_selectedFigure == null)
                _selectedFigure = fig;
            else
            {
                if (_selectedFigure == fig)
                    return;
                else
                {
                    _selectedFigure = fig;
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
                DisableEdit();
            }

            else
            {
                EnableEdit();
                PositionConfiguration.Init(zone);
            }

            _zone = zone;
            //show full view
            ColorConfiguration?.Dispose();
            ColorConfiguration = _colorConfigurationViewModelFactory.GetColorConfiguration(zone);
        }
        else
        {
            /// get all the mutual properties and display
            _zone = null;
            ColorConfiguration?.Dispose();
            _selectedFigure = null;
            DisableEdit();
        }
    }


    private void OnItemPositionChanged(float arg1, float arg2)
    {
        if (EnablePositionEdit)
            PositionConfiguration.Update();
    }

    private LightingZone _zone;
    private Draw2DCanvasViewModel _canvasViewModel;
    private PositionConfigurationViewModel _positionConfiguration;

    private ColorConfigurationViewModelFactory _colorConfigurationViewModelFactory;

    //make header reusable by set it up as singleton
    private ConfigurationHeaderViewModel _headerViewModel;

    public override void DisableEdit()
    {
        EnablePositionEdit = false;
        ColorConfiguration?.Dispose();
        ColorConfiguration = new NullColorConfigurationViewModel();
    }

    public override void EnableEdit()
    {
        EnablePositionEdit = true;
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

    public ConfigurationHeaderViewModel Header => _headerViewModel;
}
