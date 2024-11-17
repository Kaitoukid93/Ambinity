using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace AmbinityCore.Repositories;

public class Animation : ObservableObject, ICollectableItem
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


    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for color palette
        return null;
    }

    [JsonIgnore] public string LocalPath { get; set; }

    public string Description { get; set; }

    //uid will be used for resolve animation when profile is loaded
    public Guid UID { get; set; }

    public Animation(string name)
    {
        Name = name;
        UID = Guid.NewGuid();
    }

    public Animation()
    {
    }

    /// <summary>
    /// load animation frame into ram
    /// </summary>
    public void LoadAnimation()
    {
        if (!File.Exists(Path.Combine(LocalPath, "config.json")))
        {
            //todo revert to default animation
            Log.Error("Animation file not found: " + Path.Combine(LocalPath, "config.json"));
            return;
        }

        var json = File.ReadAllText(Path.Combine(LocalPath, "config.json"));
        SkottieAnimation = SkiaSharp.Skottie.Animation.Parse(json);
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