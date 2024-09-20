using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneRepository : CollectableItemRepository
{
    private readonly AnimationsRepository _animationsRepository;

    private string dbPath => Path.Combine(Constants.AppDataFolder, "Data");
    private string FolderPath => Path.Combine(dbPath, "Zones");

    public LightingZoneRepository( AnimationsRepository animationsRepository)
    {
        _animationsRepository = animationsRepository;
        LocalFolderPath = FolderPath;
        Name = "Lighting Zone";
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
            zone.LocalPath = file;
            AddItem(zone);
        }
    }

    /// <summary>
    /// create default Profile Categories
    /// </summary>
    private void CreateDefaultLightingZone()
    {
        AddItem(GetDefaultAmbilightZone("Big Ambilight",0,0,200,100,0));
        AddItem(GetDefaultAmbilightZone("Smalll Ambilight",0,0,50,50,1));
        AddItem(GetDefaultSolidColorZone("Solid Red",0,0,200,200,Avalonia.Media.Colors.Red));
        AddItem(GetDefaultSolidColorZone("Solid Greed",0,0,100,100,Avalonia.Media.Colors.GreenYellow));
        AddItem(GetDefaultColorPaletteZone("Retro Palette",0,0,200,100,DefaultColorPalettes.RetroPalette()));
        AddItem(GetDefaultColorPaletteZone("Red Palette",0,0,100,200,DefaultColorPalettes.RetroPalette()));
    }

    public LightingZone GetDefaultAmbilightZone(string name,int x, int y, int width, int height, int screenIndex)
    {
        var fullScreenAmbilightZone = new LightingZone(x, y, width, height);
        fullScreenAmbilightZone.Name = name;
        fullScreenAmbilightZone.LightingConfiguration = new ScreenCaptureConfiguration(80,
            3,
            new CaptureArea(0, 0, 1, 1),
            0,
            false);
        return fullScreenAmbilightZone;
    }
   
    public LightingZone GetDefaultSolidColorZone(string name,int x, int y, int width, int height, Color color)
    {
        var solidRedZone = new LightingZone(x, y, width, height);
        solidRedZone.Name = name;
        solidRedZone.LightingConfiguration = new SelfGeneratedColorConfiguration( new List<Color>(){color}, new NoneMotionConfiguration());
        return solidRedZone;
    }
    public LightingZone GetDefaultColorPaletteZone(string name,int x, int y, int width, int height, ColorPalette palette)
    {
        var solidRedZone = new LightingZone(x, y, width, height);
        solidRedZone.Name = name;
        solidRedZone.LightingConfiguration = new SelfGeneratedColorConfiguration( palette.Colors.ToList(), new NoneMotionConfiguration());
        return solidRedZone;
    }
    public LightingZone GetDefaultAnimationZone(string name,int x, int y, int width, int height)
    {
        var animationZone = new LightingZone(x, y, width, height);
        animationZone.Name = name;
        var animation = _animationsRepository.Items.First();
        animationZone.LightingConfiguration = new AnimationConfiguration(animation as Animation);
        
        return animationZone;
    }
}