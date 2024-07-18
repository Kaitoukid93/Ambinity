using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using Avalonia;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneRepository : CollectableItemRepository
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string dbPath => Path.Combine(JsonPath, "Data");
    private string FolderPath => Path.Combine(dbPath, "Zones");

    public LightingZoneRepository()
    {
        LocalFolderPath = FolderPath;
        Init();
    }


    public override void CreateDefault()
    {
        CreateDefaultLightingZone();
    }

    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] files = Directory.GetFiles(FolderPath);
        foreach (var file in files)
        {
            var zone = JsonHelpers.DeserializeJson<LightingZone>(file);
            if (zone == null)
                continue;
            Items.Add(zone);
        }
    }

    /// <summary>
    /// create default Profile Categories
    /// </summary>
    private void CreateDefaultLightingZone()
    {
        Items.Add(GetDefaultAmbilightZone());
    }

    public LightingZone GetDefaultAmbilightZone()
    {
        var fullScreenAmbilightZone = new LightingZone(200, 200, 200, 200);
        fullScreenAmbilightZone.Name = "Full Screen Ambilight";
        fullScreenAmbilightZone.LightingConfiguration = new ScreenCaptureConfiguration(80,
            3,
            new Rect(0, 0, 1, 1),
            0,
            false);
        return fullScreenAmbilightZone;
    }
    public LightingZone GetDefaultSolidColorZone()
    {
        var solidRedZone = new LightingZone(0,0, 200, 200);
        solidRedZone.Name = "Solid Red";
        solidRedZone.LightingConfiguration = new StaticColorConfiguration();
        return solidRedZone;
    }
}