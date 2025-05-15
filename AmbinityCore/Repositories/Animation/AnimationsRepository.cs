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
            else if (File.Exists(Path.Combine(dir, "animation.gif")))
            {
                animation = JsonHelpers.DeserializeJson<GifAnimation>(Path.Combine(dir, "animation.json"));
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
    /// Removes all animation directories on disk.
    /// </summary>
    public void ClearAllAnimations()
    {
        if (!Directory.Exists(LocalFolderPath))
        {
            return; // If the folder doesn't exist, there's nothing to clear
        }

        // Get all directories under the LocalFolderPath
        string[] directories = Directory.GetDirectories(LocalFolderPath);

        foreach (var dir in directories)
        {
            try
            {
                // Delete each directory and its contents
                Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions if needed
                Console.WriteLine($"Failed to delete directory {dir}: {ex.Message}");
            }
        }

        // Clear the in-memory collection
        Items?.Clear();
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
