using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;

namespace Ambinity.Views.AmbinityStore;

public class RepositoryHelpers(
    LightingZoneRepository lightingZoneRepository,
    AnimationsRepository animationsRepository,
    ColorPaletteRepository colorPaletteRepository,
    LightingProfileRepository lightingProfileRepository)
{
   

    public void AddItemToRepository(ICollectableItem item)
    {
        if (item is LightingZone)
        {
            lightingZoneRepository.AddItem(item);
        }
        if (item is LightingProfile)
        {
            lightingProfileRepository.AddItem(item);
        }
        if (item is ColorPalette)
        {
            colorPaletteRepository.AddItem(item);
        }
        if (item is Animation)
        {
            animationsRepository.AddItem(item);
        }
    }
}