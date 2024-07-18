using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Profile;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
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

    /// <summary>
    /// Indicate Item is selected by user
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Indicate Item is being edited by user
    /// </summary>
    public bool IsEditing { get; set; }

    /// <summary>
    /// Indicate Item is being checked by user
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Indicate Item is being Pinned to dashboard
    /// </summary>
    public bool IsPinned { get; set; }

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

    public void AddProfile(LightingProfile profile)
    {
        profile.CategoryID = ID;
        Profiles.Add(profile);
    }

    /// <summary>
    /// get child from existed profile list
    /// </summary>
    /// <param name="profiles"></param>
    public void FindChild(List<ICollectableItem> profiles)
    {
        if (profiles == null)
            return;
        foreach (LightingProfile profile in profiles)
        {
            if (profile == null)
                continue;
            if (profile.CategoryID == ID)
            {
                Profiles.Add(profile);
            }

            if (profile.IsDefault && IsDefault)
            {
                Profiles.Add(profile);
            }
        }
    }
}