using System.Linq;
using Ambinity.ViewModels;
using Ambinity.Views.Configuration.ColorConfiguration;
using Ambinity.Views.Configuration.PositionConfiguration;
using Ambinity.Views.Draw2DCanvas;
using Ambinity.Views.Screens.ProfileEditor.ZoneConfiguration;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using Draw2D.Core;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZonePropertiesViewModel : CanvasObjectPropertiesViewModelBase
{
    public ZonePropertiesViewModel(Draw2DCanvasViewModel canvasViewModel)
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
            var zone = (fig as LightingZoneFigure).ChildItem as LightingZone;
            if(zone ==null || !fig.IsResizable)
                DisableEdit();
            else
            {
                
                PositionConfiguration = new PositionConfigurationViewModel(_canvasViewModel);
                PositionConfiguration.Init(zone);
            }
            //show full view
            
            ColorConfiguration = ColorConfigurationHelper.GetColorConfiguration(zone.LightingConfiguration);
            Header = GetHeader(zone.LightingConfiguration);
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
    private PropertiesViewHeader GetHeader(ILightingConfiguration config)
    {
        var header = new PropertiesViewHeader(null, null);
        switch (config.Type)
        {
            case ConfigurationType.ScreenCapture:
                header.Icon = config.Icon;
                header.Header = "Screen Capture";
                break;
            
            case ConfigurationType.ColorPalette:
                header.Header = "Color Palette";
                header.Icon = config.Icon;
                break;
            
            case ConfigurationType.StaticColor:
                header.Header = "Fill Color";
                header.Icon = config.Icon;
                break;
            
            case ConfigurationType.MusicReactive:
                header.Header = "Music Reactive";
                header.Icon = config.Icon;
                break;
            
            case ConfigurationType.Gifxelation:
                header.Header = "Gifxelation";
                header.Icon = config.Icon;
                break;
            
            case ConfigurationType.Animation:
                header.Header = "Animation";
                header.Icon = config.Icon;
                break;
        }
        return header;
        
    }
    private PropertiesViewHeader NullHeader()
    {
        return new PropertiesViewHeader("0 item selected", null,false);
    }
    private PropertiesViewHeader MultipleSelectedHeader(int count)
    {
        return new PropertiesViewHeader(count+" "+"items selected", "Zones");
    }
    
}