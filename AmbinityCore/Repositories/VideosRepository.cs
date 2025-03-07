using System;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;

namespace AmbinityCore.Repositories;

public class VideosRepository : CollectableItemRepository
{

    private string FolderPath => Path.Combine(Constants.AppDataFolder, "Data", "Video");

    public VideosRepository()
    {
        Name = "Video";
        LocalFolderPath = FolderPath;
    }

    public VideosRepository(string localPath)
    {
        Name = "Video";
        LocalFolderPath = localPath;
    }

    public override void CreateDefault()
    {
        CreateDefaultVideo();
    }


    public override void LoadFromDisk()
    {
        Items?.Clear();
        string[] directories = Directory.GetDirectories(LocalFolderPath);
        foreach (var dir in directories)
        {
            var video = JsonHelpers.DeserializeJson<Video>(Path.Combine(dir, "video.json"));
            if (video == null)
                continue;
            if (video.UID == null || video.UID == Guid.Empty)
                video.UID = Guid.NewGuid();
            video.LocalPath = dir;
            video.LocalRepository = this;
            AddItem(video);
        }
    }

    /// <summary>
    /// create default Solid Colors
    /// </summary>
    private void CreateDefaultVideo()
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

    public Video FindVideo(Guid videoUID)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as Video).UID == videoUID).FirstOrDefault() as Video;
    }

    public Video FindVideo(string videoName)
    {
        if (Items.Count == 0)
            return null;
        return Items.Where(i => (i as Video).Name == videoName).FirstOrDefault() as Video;
    }
}
