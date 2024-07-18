using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string dbPath => Path.Combine(JsonPath, "Data");
    private string FolderPath => Path.Combine(dbPath, "Profiles");
    private LightingZoneRepository _zoneRepository;

    public LightingProfileRepository( LightingZoneRepository zoneRepository)
    {
        LocalFolderPath = FolderPath;
        _zoneRepository = zoneRepository;
        Init();
    }


    public override void CreateDefault()
    {
        CreateDefaultProfiles();
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            var profile = JsonHelpers.DeserializeJson<LightingProfile>(file);
            if (profile == null)
                continue;
            Items.Add(profile);
        }
    }

    /// <summary>
    /// create default Profile Categories
    /// </summary>
    private void CreateDefaultProfiles()
    {
        //create default ambilight profile
        var ambilightProfile = new LightingProfile()
        {
            Name = "Ambilight",
            Icon = "Ambilight_addzone",
            ID = Guid.NewGuid(),
            IsDefault = true,
        };
        var solidColorProfile = new LightingProfile()
        {
            Name = "Solid",
            Icon = "Color_bucket",
            ID = Guid.NewGuid(),
            IsDefault = true,
        };
        ambilightProfile.Zones.Add(_zoneRepository.GetDefaultAmbilightZone());
        solidColorProfile.Zones.Add(_zoneRepository.GetDefaultSolidColorZone());
        Items.Add(ambilightProfile);
        Items.Add(solidColorProfile);
        //write this to disk
    }
}