using System.Collections.ObjectModel;
using System.ComponentModel;
using AmbinityCore.Helpers;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;


namespace AmbinityCore.Models.Profile;

public class LightingProfile : ObservableObject, IDisposable, ICollectableItem
{
    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public event Action<LightingZone>? LightingZoneAdded;
    public event Action<LightingZone>? LightingZoneRemoved;
    public event Action<ICollectableItem>? IconChanged;

    public LightingProfile()
    {
        //todo implement file save client to save when zones property changed
        Zones = new ObservableCollection<LightingZone>();
    }

    /// <summary>
    /// get or set category
    /// </summary>
    [JsonIgnore]
    public LightingProfileCategory Category { get; set; }

    /// <summary>
    /// Name of the profile
    /// </summary>
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            ItemNameChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// Display Icon type of this profile
    /// </summary>
    public IconTypeEnum IconType { get; set; }

    /// <summary>
    /// Display Icon of this profile
    /// </summary>
    public string Icon { get; set; }

    public void UpdateIcon()
    {
        IconChanged?.Invoke(this);
    }

    /// <summary>
    /// Describe the profile
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// get playing status of this profile
    /// </summary>
    private bool _isPlaying;

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    /// <summary>
    /// get number of zones
    /// </summary>
    [JsonIgnore]
    public int ZoneCount => Zones.Count;

    /// <summary>
    /// Unique ID of parrent category
    /// </summary>
    public Guid CategoryID { get; set; }

    /// <summary>
    /// unique ID of this profile
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// Indicate Item is selected by user
    /// </summary>
    [JsonIgnore]
    public bool IsSelected { get; set; }

    /// <summary>
    /// Indicate Item is being edited by user
    /// </summary>
    [JsonIgnore]
    public bool IsEditing { get; set; }

    /// <summary>
    /// Indicate Item is being checked by user
    /// </summary>
    [JsonIgnore]
    public bool IsChecked { get; set; }

    /// <summary>
    /// Indicate Item is being Pinned to dashboard
    /// </summary>
    [JsonIgnore]
    public bool IsPinned { get; set; }

    /// <summary>
    /// Store Local path of this item
    /// </summary>
    [JsonIgnore]
    public string LocalPath { get; set; }

    /// <summary>
    /// Indicate if this is default item
    /// </summary>
    [JsonIgnore]
    public bool Disposed { get; set; }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<LightingProfileRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        //todo make online repo for lighting profile controller
        return null;
    }

    public bool IsDefault { get; set; }
    public ObservableCollection<LightingZone> Zones { get; set; }
    private ColorEngineProvider _colorEngineProvider;

    /// <summary>
    /// save configuration to disk
    /// </summary>
    public void Save()
    {
        //todo implement profile save with icon 
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = GetLocalRepository().LocalFolderPath;
            LocalPath = Path.Combine(dbPath, ID.ToString());
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "profile.json"));
    }

    /// <summary>
    /// save this profile to a folder with the name "profile.json
    /// </summary>
    /// <param name="path"></param>
    public void SaveTo(string path)
    {
        JsonHelpers.WriteSimpleJson(this, Path.Combine(path, "profile.json"));
    }

    /// <summary>
    /// add new lighting zone
    /// </summary>
    public void AddLightingZone(LightingZone zone)
    {
        Zones.Add(zone);
        LightingZoneAdded?.Invoke(zone);
    }

    private void ZonePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(sender));
    }

    /// <summary>
    /// delete lighting zone
    /// </summary>
    /// <param name="zone"></param>
    public void RemoveLightingZone(LightingZone zone)
    {
        Zones.Remove(zone);
        LightingZoneRemoved?.Invoke(zone);
    }

    public void TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        //clear lighting zone
        //suspend thread
        Disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}