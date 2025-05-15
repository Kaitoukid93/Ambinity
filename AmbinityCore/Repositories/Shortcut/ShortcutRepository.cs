using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;

namespace AmbinityCore.Repositories;

public class ShortcutRepository : CollectableItemRepository
{
    private string FolderPath => Path.Combine(Constants.AppDataFolder, "Data", "Shortcuts");
    private LightingProfileRepository _lightingProfileRepository;

    public ShortcutRepository(LightingProfileRepository profileRepository)
    {
        Name = "Shortcut";
        LocalFolderPath = FolderPath;
        _lightingProfileRepository = profileRepository;
    }

    public override void CreateDefault()
    {
        CreateDefaultShortcut();
    }


    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(LocalFolderPath);
        foreach (var dir in directories)
        {
            var shortcut = JsonHelpers.DeserializeJson<Shortcut>(Path.Combine(dir, "shortcut.json"));
            if (shortcut == null)
                continue;
            shortcut.LocalPath = dir;
            AddItem(shortcut);
        }
    }

    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultShortcut()
    {
        // AddItem(DefaultColorPalettes.RetroPalette());
        AddItem(new Shortcut()
        {
            Name = "Ambilight",
            Description = "Shortcut for ambilight mode",
            IsDefault = true,
            Icon = "ambilight",
        });
        AddItem(new Shortcut()
        {
            Name = "Neon",
            Description = "Shortcut for rainbow effect",
            IsDefault = true,
            Icon = "rainbow"
        });
        AddItem(new Shortcut()
        {
            Name = "Solid Blue",
            Description = "Ambinity default solid blue profile",
            IsDefault = true,
            Icon = "genericCircle"
        });


        //find child for each shortcut if profile is not attached
        foreach (var item in Items)
        {
            var shortcut = item as Shortcut;
            if (shortcut.LightingProfileID == null || shortcut.LightingProfileID == Guid.Empty)
            {
                LightingProfile profile = _lightingProfileRepository.Items.SingleOrDefault(i => i.Name == shortcut.Name) as LightingProfile;
                if (profile != null)
                    shortcut.LightingProfileID = profile.ID;
                shortcut.Save();
            }
        }
    }

    public override void ImportItem(string path)
    {
        //simply copy folder to repository folder path
        LocalFileHelpers.CopyDirectory(path, LocalFolderPath, true);
        LoadFromDisk();
        //update the collection
    }
}
