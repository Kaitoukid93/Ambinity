using Newtonsoft.Json;
using AmbinityCore.Models.Profile;

namespace AmbinityCore.Repositories;

public sealed class LightingProfileRepository
    : ItemRepository<LightingProfile>
{
    protected override string AssetType => AssetTypes.Profile;
    public LightingProfileRepository(
        string libraryRoot,
        LocalAssetIndexService indexService)
        : base(Path.Combine(libraryRoot, "Profiles"), indexService)
    {
    }

    protected override void SaveItem(LightingProfile profile)
    {
        var profilePath = Path.Combine(BasePath, profile.ID.ToString());
        Directory.CreateDirectory(profilePath);

        File.WriteAllText(
            Path.Combine(profilePath, "config.json"),
            JsonConvert.SerializeObject(profile, Formatting.Indented));
    }

    protected override LightingProfile LoadItem(string id)
    {
        var profilePath = Path.Combine(BasePath, id, "config.json");
        if (!File.Exists(profilePath))
            throw new FileNotFoundException("Profile not found", profilePath);

        var profile = JsonConvert.DeserializeObject<LightingProfile>(
            File.ReadAllText(profilePath))!;

        profile.LocalPath = profilePath;
        return profile;
    }

    protected override void DeleteItem(string id)
    {
        var profilePath = Path.Combine(BasePath, id);
        if (Directory.Exists(profilePath))
            Directory.Delete(profilePath, true);
    }

    protected override AssetDescriptor CreateDescriptor(LightingProfile profile)
    {
        return new AssetDescriptor
        {
            Id = profile.ID.ToString(),
            AssetType = "Profile",
            Name = profile.Name,
            Tags = profile.Tags?.ToList() ?? new(),
            RelativePath = $"Profiles/{profile.ID}/",
            Thumbnail = "icon.png",
            Source = AssetSource.Local
        };
    }
}
