using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;

namespace AmbinityCore.Models.ProfileCategory;

public class LightingProfileCategory : ObservableObject, ICollectableItem
{
    public LightingProfileCategory()
    {
        Profiles = new List<LightingProfile>();
    }

    private string _name = "New Category";

    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;

    /// <summary>
    /// display text content of this item
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
    public string Description { get; set; }
    /// <summary>
    /// Indicate Item is selected by user
    /// </summary>
    [JsonIgnore] public bool IsSelected { get; set; }

    /// <summary>
    /// Indicate Item is being edited by user
    /// </summary>
    [JsonIgnore] public bool IsEditing { get; set; }

    /// <summary>
    /// Indicate Item is being checked by user
    /// </summary>
    [JsonIgnore] public bool IsChecked { get; set; }

    /// <summary>
    /// Indicate Item is being Pinned to dashboard
    /// </summary>
    [JsonIgnore] public bool IsPinned { get; set; }
    
    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

    public OnlineItemRepository GetOnlineRepository()
    {
        //todo make online repo for lighting profile controller
        return null;
    }
    /// <summary>
    /// Store Local path of this item
    /// </summary>
    public string LocalPath { get; set; }

    /// <summary>
    /// Indicate if this is default item
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Unique ID of this category
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// profile that has been added to this category
    /// </summary>
    [JsonIgnore]
    public List<LightingProfile> Profiles { get; set; }

    public void RemoveProfile(LightingProfile profile)
    {
        Profiles.Remove(profile);
        OnPropertyChanged(nameof(Profiles));
        Save();
    }
    public void AddProfile(LightingProfile profile)
    {
        profile.CategoryID = ID;
        Profiles.Add(profile);
        OnPropertyChanged(nameof(Profiles));
        profile.Save();
        Save();
    }

    /// <summary>
    /// get child from existed profile list
    /// </summary>
    /// <param name="profiles"></param>
    public void FindChild(List<ICollectableItem> profiles)
    {
        Profiles?.Clear();
        if (profiles == null)
            return;
        foreach (LightingProfile profile in profiles)
        {
            if (profile == null)
                continue;
            if (profile.CategoryID == ID)
            {
                profile.Category = this;
                Profiles.Add(profile);
                continue;
            }

            if (profile.IsDefault && IsDefault)
            {
                profile.Category = this;
                Profiles.Add(profile);
            }
        }
    }

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.Unknown;
    }

    public void Save()
    {
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = LocalRepository.LocalFolderPath;
            LocalPath = Path.Combine(dbPath, Name + ".json"); // item without thumbnaill will be store in the same folder
        }
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}