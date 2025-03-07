using System;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Repositories;
/// <summary>
/// Represent a video source for canvas decoration
/// </summary>
public class Video : ObservableObject, ICollectableItem
{
 public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }


    public OnlineItemRepository GetOnlineRepository()
    {
        //todo make online repo for color palette
        return null;
    }

    [JsonIgnore] public string LocalPath { get; set; }

    public string Description { get; set; }

    //uid will be used for resolve video when profile is loaded
    public Guid UID { get; set; }

    public Video(string name)
    {
        Name = name;
        UID = Guid.NewGuid();
    }

    public Video()
    {
    }

    /// <summary>
    /// load animation frame into ram
    /// </summary>
    public void LoadVideo()
    {
        if (!File.Exists(Path.Combine(LocalPath, "config.json")))
        {
            //todo revert to default animation
            Log.Error("Video file not found: " + Path.Combine(LocalPath, "config.json"));
            return;
        }

        //parse video to frame using ffmpeg
    }

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.Animation;
    }

    public void Save()
    {
        //create local path
        var dbPath = LocalRepository.LocalFolderPath;
        LocalPath = Path.Combine(dbPath, Name);
        Directory.CreateDirectory(LocalPath);
        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "animation.json"));
    }

    public void Export(string path)
    {
        JsonHelpers.WriteSimpleJson(this, path);
    }

    [JsonIgnore] public SkiaSharp.Skottie.Animation SkottieAnimation { get; set; }
}
