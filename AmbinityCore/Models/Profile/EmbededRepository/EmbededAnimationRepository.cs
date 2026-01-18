
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmbinityCore.Helpers;
using AmbinityCore.Repositories;
namespace AmbinityCore.Models.Profile;

public sealed class EmbeddedAnimationRepository
    : IEmbeddedAssetRepository<IAnimation>
{
    private readonly string _path;

    public EmbeddedAnimationRepository(string assetsRoot)
    {
        _path = Path.Combine(assetsRoot, "animations");
    }

    public IReadOnlyList<IAnimation> LoadAll()
    {
        if (!Directory.Exists(_path))
            return [];
        string[] directories = Directory.GetDirectories(_path);
        var animations = new List<IAnimation>();
        foreach (var dir in directories)
        {
            IAnimation animation = null;

            if (File.Exists(Path.Combine(dir, "config.json")))
            {
                animation = JsonHelpers.DeserializeJson<LottieJsonAnimation>(Path.Combine(dir, "animation.json"));
            }
            else if (File.Exists(Path.Combine(dir, "animation.gif")))
            {
                animation = JsonHelpers.DeserializeJson<GifAnimation>(Path.Combine(dir, "animation.json"));
            }
            if (animation == null)
                continue;
            if (animation.UID == null || animation.UID == Guid.Empty)
                animation.UID = Guid.NewGuid();
            animation.LocalPath = dir;
            // animation.LocalRepository = this;
            animations.Add(animation);
        }
        return animations;
    }

}
