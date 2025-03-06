using System.Collections.ObjectModel;
using System.ComponentModel;
using AmbinityCore.Helpers;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Controller;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.ProfileCategory;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using OpenRGB.NET;
using Serilog;
using Color = Avalonia.Media.Color;


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

    public Color IconColor { get; set; }

    /// <summary>
    /// Brightness for each profile, this will not override device brightness but
    /// instead apply a factor
    /// </summary>
    public int Brightness { get; set; } = 100;

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

    [JsonIgnore]
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

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

    public OnlineItemRepository GetOnlineRepository()
    {
        //todo make online repo for lighting profile controller
        return null;
    }

    public bool IsDefault { get; set; }
    public ObservableCollection<LightingZone> Zones { get; set; }
    private ColorEngineProvider _colorEngineProvider;

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.LightingProfile;
    }

    /// <summary>
    /// save configuration to disk
    /// </summary>
    public void Save()
    {
        //todo implement profile save with icon 
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = LocalRepository.LocalFolderPath;
            LocalPath = Path.Combine(dbPath, ID.ToString());
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "config.json"));
    }

    /// <summary>
    /// save this profile to a folder with the name "profile.json
    /// </summary>
    /// <param name="path"></param>
    public void SaveTo(string path)
    {
        JsonHelpers.WriteSimpleJson(this, Path.Combine(path, "config.json"));
    }

    /// <summary>
    /// add new lighting zone
    /// </summary>
    public void AddLightingZone(LightingZone zone)
    {
        zone.ParentProfile = this;
        //create repository if needed
        UpdateRepository(zone.LightingConfiguration.Type);
        Zones.Add(zone);
        LightingZoneAdded?.Invoke(zone);
    }

    public void UpdateRepository(ConfigurationType type)
    {
        if (Assets == null)
            Assets = new List<CollectableItemRepository>();
        switch (type)
        {
            case ConfigurationType.Animation:
                if (AnimationRepository == null)
                    Assets.Add(new AnimationsRepository(Path.Combine(_assetPath, "animations")));
                break;
        }
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

    public string AssetPath => _assetPath;
    private string _assetPath => Path.Combine(LocalPath, "assets");

    public void LoadAssets()
    {
        Assets = [];
        if (!Directory.Exists(_assetPath))
        {
            Log.Information("Profile has no assets");
            return;
        }

        string[] directories = Directory.GetDirectories(_assetPath);
        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);
            switch (dirName)
            {
                case "animations":
                    var repo = new AnimationsRepository(dir);
                    repo.LoadFromDisk();
                    Assets.Add(repo);
                    Log.Information("Animation asset loaded");
                    break;
            }
        }
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

    [JsonIgnore] public List<CollectableItemRepository> Assets { get; set; }

    [JsonIgnore]
    public AnimationsRepository AnimationRepository =>
        Assets?.Where(a => a.Name == "Animation").FirstOrDefault() as AnimationsRepository;
}