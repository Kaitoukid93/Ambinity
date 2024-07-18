using AmbinityCore.Models.Lighting.Zone;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZoneLayerViewModel : LayerViewModelBase
{
    public ZoneLayerViewModel(LightingZoneFigure zone)
    {
        _zoneFigure = zone;
        Name = _zoneFigure.Zone.Name;
        
    }

    private LightingZoneFigure _zoneFigure;
  
}