using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileRepository : CollectableItemRepository
{

    private string dbPath => Path.Combine(Constants.AppDataFolder, "Data");
    private string FolderPath => Path.Combine(dbPath, "Profiles");
    private LightingZoneRepository _zoneRepository;

    public LightingProfileRepository(LightingZoneRepository zoneRepository)
    {
        LocalFolderPath = FolderPath;
        _zoneRepository = zoneRepository;
        Name = "Lighting Profiles";

    }


    public override void CreateDefault()
    {
        CreateDefaultProfiles();
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] files = Directory.GetDirectories(FolderPath);
        foreach (var file in files)
        {
            var profilePath = Path.Combine(file, "profile.json");
            var profile = JsonHelpers.DeserializeJson<LightingProfile>(profilePath);
            if (profile == null)
                continue;
            profile.LocalPath = profilePath;
            AddItem(profile);
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
            Icon = "Youtube",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        var solidColorProfile = new LightingProfile()
        {
            Name = "Solid Green",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        var colorPaletteProfile  = new LightingProfile()
        {
            Name = "Color palette",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        var musicReactive  = new LightingProfile()
        {
            Name = "Music Reactive",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        var animation  = new LightingProfile()
        {
            Name = "Animation",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            IsDefault = true
        };
        ambilightProfile.TogglePlayPause();
        ambilightProfile.Zones.Add(_zoneRepository.GetDefaultAmbilightZone("Big Ambilight",0,0,200,100,0));
        ambilightProfile.Zones.Add(_zoneRepository.GetDefaultAmbilightZone("Smalll Ambilight",0,0,50,50,1));
        solidColorProfile.Zones.Add(_zoneRepository.GetDefaultSolidColorZone("Solid Red",0,0,200,200,Avalonia.Media.Colors.Red));
        solidColorProfile.Zones.Add(_zoneRepository.GetDefaultSolidColorZone("Solid Greed",0,0,100,100,Avalonia.Media.Colors.GreenYellow));
        colorPaletteProfile.Zones.Add(_zoneRepository.GetDefaultColorPaletteZone("Retro Palette",0,0,200,100,DefaultColorPalettes.RetroPalette()));
        colorPaletteProfile.Zones.Add(_zoneRepository.GetDefaultColorPaletteZone("Red Palette",0,0,100,200,DefaultColorPalettes.RetroPalette()));
        colorPaletteProfile.Zones.Add(_zoneRepository.GetDefaultAnimationZone("Demo",100,100,200,100,null));
        AddItem(ambilightProfile);
        AddItem(solidColorProfile);
        AddItem(colorPaletteProfile);
        AddItem(musicReactive);
        AddItem(animation);
        
        //write this to disk
    }
}