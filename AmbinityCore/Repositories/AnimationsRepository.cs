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

    public AnimationsRepository(string localPath)
    {
        Name = "Animation";
        LocalFolderPath = localPath;
    }

    public override void CreateDefault()
    {
        CreateDefaultAnimation();
    }


    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(LocalFolderPath);
        foreach (var dir in directories)
        {
            var animation = JsonHelpers.DeserializeJson<Animation>(Path.Combine(dir, "animation.json"));
            if (animation == null)
                continue;
            if (animation.UID == null || animation.UID == Guid.Empty)
                animation.UID = Guid.NewGuid();
            animation.LocalPath = dir;
            animation.LocalRepository = this;
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
        LocalFileHelpers.CopyDirectory(path, LocalFolderPath, true);
        LoadFromDisk();
        //update the collection
    }

    public Animation FindAnimation(Guid animationUID)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as Animation).UID == animationUID).FirstOrDefault() as Animation;
    }

    public Animation FindAnimation(string name)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as Animation).Name == name).FirstOrDefault() as Animation;
    }
}