using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;

namespace AmbinityCore.Repositories;

public class ShortcutRepository: CollectableItemRepository
{
    
    private string FolderPath => Path.Combine(Constants.AppDataFolder, "Data", "Shortcuts");

    public ShortcutRepository()
    {
        Name = "Shortcut";
        LocalFolderPath = FolderPath;
    }
    public override void CreateDefault()
    {
        CreateDefaultShortcut();
    }

   
    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(FolderPath);
        foreach (var dir in directories)
        {

            var shortcut = JsonHelpers.DeserializeJson<Shortcut>(Path.Combine(dir,"shortcut.json"));
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
            Icon = "ambilight"
            
        });
        AddItem(new Shortcut()
        {
            Name = "Audio Reactive",
            Description = "Shortcut for music mode",
            IsDefault = true,
            Icon = "headphones"
            
        });
        AddItem(new Shortcut()
        {
            Name = "Rainbow",
            Description = "Shortcut for rainbow effect",
            IsDefault = true,
            Icon = "rainbow"
            
        });
        AddItem(new Shortcut()
        {
            Name = "Mix",
            Description = "Ambinity default mix profile",
            IsDefault = true,
            Icon = "puzzle"
            
        });
        AddItem(new Shortcut()
        {
            Name = "Custom 1",
            Description = "User define shortcut",
            IsDefault = true,
            Icon = "user"
            
        });
        AddItem(new Shortcut()
        {
            Name = "Custom 2",
            Description = "User define shortcut",
            IsDefault = true,
            Icon = "user"
            
        });
    }
    public override void ImportItem(string path)
    {
        //simply copy folder to repository folder path
        LocalFileHelpers.CopyDirectory(path,LocalFolderPath,true);
        LoadFromDisk();
        //update the collection
        
    }
}