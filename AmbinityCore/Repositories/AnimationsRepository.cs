using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;

namespace AmbinityCore.Repositories;

public class AnimationsRepository : CollectableItemRepository
{

    private string FolderPath => Path.Combine(Constants.AppDataFolder, "Data", "Animation");

    public AnimationsRepository()
    {
        Name = "Animation";
        LocalFolderPath = FolderPath;
    }
    public override void CreateDefault()
    {
        CreateDefaultAnimation();
    }

   
    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(FolderPath);
        foreach (var dir in directories)
        {

            var animation = JsonHelpers.DeserializeJson<Animation>(Path.Combine(dir,"animation.json"));
            if (animation == null)
                continue;
            animation.LocalPath = dir;
            AddItem(animation);
        }
    }
    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultAnimation()
    {
        // AddItem(DefaultColorPalettes.RetroPalette());
    }
    public override void ImportItem(string path)
    {
        //simply copy folder to repository folder path
        LocalFileHelpers.CopyDirectory(path,LocalFolderPath,true);
        LoadFromDisk();
        //update the collection
        
    }
}