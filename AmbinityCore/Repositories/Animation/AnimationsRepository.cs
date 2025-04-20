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
            IAnimation animation = null;

            if (File.Exists(Path.Combine(dir, "config.json")))
            {
                animation = JsonHelpers.DeserializeJson<LottieJsonAnimation>(Path.Combine(dir, "animation.json"));
            }
            else if (File.Exists(Path.Combine(dir, "video.mp4")))
            {
                animation = JsonHelpers.DeserializeJson<VideoAnimation>(Path.Combine(dir, "animation.json"));
            }
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

    public IAnimation FindAnimation(Guid animationUID)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as IAnimation).UID == animationUID).FirstOrDefault() as IAnimation;
    }

    public IAnimation FindAnimation(string name)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as IAnimation).Name == name).FirstOrDefault() as IAnimation;
    }
}
