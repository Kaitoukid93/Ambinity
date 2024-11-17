using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;

namespace AmbinityCore.Repositories;

public class CollectableItemPackingService
{
    private readonly AnimationsRepository _animationRepository;
    private readonly ColorPaletteRepository _paletteRepository;

    public CollectableItemPackingService(ColorPaletteRepository paletteRepository,
        AnimationsRepository animationsRepository)
    {
        _paletteRepository = paletteRepository;
        _animationRepository = animationsRepository;
    }

    public void Pack(ICollectableItem item)
    {
        Directory.CreateDirectory(Path.Combine(item.LocalPath, "assets"));
        if (item is LightingProfile profile)
        {
            //create assets folder
            var hasAnimationZone = profile.Zones.Any(z => z.LightingConfiguration.Type == ConfigurationType.Animation);
            if (hasAnimationZone)
            {
                Directory.CreateDirectory(Path.Combine(item.LocalPath, "assets", "animations"));
            }
            else
            {
                return;
            }

            var assets = new List<Guid>();
            foreach (var zone in profile.Zones)
            {
                if (zone.LightingConfiguration.Type == ConfigurationType.Animation)
                {
                    var animationConfiguration = zone.LightingConfiguration as AnimationConfiguration;
                    if(!assets.Contains(animationConfiguration.AnimationUID))
                    assets.Add(animationConfiguration.AnimationUID);
                }
            }
            if(assets.Count ==0)
                return;
            foreach (var asset in assets)
            {
                var animation = _animationRepository.FindAnimation(asset);
                LocalFileHelpers.CopyDirectory(animation.LocalPath, Path.Combine(item.LocalPath, "assets", "animations"),true);
            }
        }
    }
}