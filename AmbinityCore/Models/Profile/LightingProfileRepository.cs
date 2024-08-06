using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Repositories;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Profile;

public sealed class LightingProfileRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string dbPath => Path.Combine(JsonPath, "Data");
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
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            var profile = JsonHelpers.DeserializeJson<LightingProfile>(file);
            if (profile == null)
                continue;
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
            Name = "Theater",
            Icon = "Youtube",
            ID = Guid.NewGuid(),
            LocalPath = Path.Combine(this.FolderPath,"Theater"),
            IsDefault = true
        };
        var solidColorProfile = new LightingProfile()
        {
            Name = "Solid Red",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            LocalPath = Path.Combine(this.FolderPath,"Solid Red"),
            IsDefault = true
        };
        var colorPaletteProfile  = new LightingProfile()
        {
            Name = "Retro",
            Icon = "solidrect",
            ID = Guid.NewGuid(),
            LocalPath = Path.Combine(this.FolderPath,"Retro"),
            IsDefault = true
        };
        ambilightProfile.TogglePlayPause();
        ambilightProfile.Zones.Add(_zoneRepository.GetDefaultAmbilightZone("Big Ambilight",0,0,200,100,0));
        ambilightProfile.Zones.Add(_zoneRepository.GetDefaultAmbilightZone("Smalll Ambilight",0,0,50,50,1));
        solidColorProfile.Zones.Add(_zoneRepository.GetDefaultSolidColorZone("Solid Red",0,0,200,200,Avalonia.Media.Colors.Red));
        solidColorProfile.Zones.Add(_zoneRepository.GetDefaultSolidColorZone("Solid Greed",0,0,100,100,Avalonia.Media.Colors.GreenYellow));
        colorPaletteProfile.Zones.Add(_zoneRepository.GetDefaultColorPaletteZone("Retro Palette",0,0,200,100,DefaultColorPalettes.RetroPalette()));
        colorPaletteProfile.Zones.Add(_zoneRepository.GetDefaultColorPaletteZone("Red Palette",0,0,100,200,DefaultColorPalettes.RetroPalette()));
        AddItem(ambilightProfile);
        AddItem(solidColorProfile);
        AddItem(colorPaletteProfile);
        
        //write this to disk
    }
}