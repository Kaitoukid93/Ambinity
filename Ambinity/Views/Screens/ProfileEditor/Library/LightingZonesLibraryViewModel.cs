using Ambinity.Views.CollectableItem.LightingZone;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Screens.DeviceLayout.Library;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Screens.ProfileEditor.Library;

public class LightingZonesLibraryViewModel : LibraryViewModelBase
{
    public LightingZonesLibraryViewModel(LightingZoneRepository lightingZoneRepository,
        LightingZoneOnlineRepository onlineRepository,
        LightingZoneAssetsViewModel layoutAssetsViewModel) : base(lightingZoneRepository,
         onlineRepository, layoutAssetsViewModel)
    {
    }
}