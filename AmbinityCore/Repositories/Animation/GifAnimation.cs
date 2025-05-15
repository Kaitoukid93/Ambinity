using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityCore.Repositories;

public class GifAnimation : ObservableObject, IAnimation
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

    //uid will be used for resolve animation when profile is loaded
    public Guid UID { get; set; }
    public TimeSpan Duration { get; }
    public string Fps { get; }
    public string Size { get; } 
    public string Version { get; }

    public GifAnimation(string name)
    {
        Name = name;
        UID = Guid.NewGuid();
    }

    public GifAnimation()
    {

    }

    /// <summary>
    /// load animation frame into ram
    /// </summary>
    public void LoadAnimation()
    {
        if (!File.Exists(Path.Combine(LocalPath, "animation.gif")))
        {
            //todo revert to default animation
            Log.Error("Animation file not found: " + Path.Combine(LocalPath, "animation.gif"));
            return;
        }
        AnimationPath = Path.Combine(LocalPath,"animation.gif");

        //load animation using ffmpeg
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


    [JsonIgnore] public string AnimationPath { get; set; }
}
