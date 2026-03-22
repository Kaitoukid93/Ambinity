using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityDB.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using AmbinityCore.Repositories.Loader;
using AmbinityCore.Models.Profile.Service;
using System.Drawing;
namespace AmbinityCore.Models.Profile;

/// <summary>
/// This class use to work with viewmodel, when you want to display
/// list or single lighting profile
/// </summary>
public class LightingProfileItem : ObservableObject, ICollectableItem
{
    private  ManifestEntry _entry;
    private readonly IAssetLoader _loader;
    private readonly IPlayerService _player;

    private LightingProfile _data;
    private Task<LightingProfile> _loadTask;

    public LightingProfileItem(
        ManifestEntry entry,
        IAssetLoader loader,
        IPlayerService player)
    {
        _entry = entry;
        _loader = loader;
        _player = player;
    }

    // 🔹 Metadata (instant)
    public string Name => _entry.Name;
    public string Icon => _entry.Icon;
    public string Thumbnail => _entry.Thumbnail;
    public string Id => _entry.Id;
    // 🔹 Lazy load full data
    public Task<LightingProfile> GetDataAsync()
    {
        if (_loadTask != null)
            return _loadTask;

        _loadTask = LoadInternalAsync();
        return _loadTask;
    }

     public void Update(ManifestEntry entry)
    {
        _entry = entry;

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Icon));
    }

    private async Task<LightingProfile> LoadInternalAsync()
    {
        _data = await _loader.LoadAsync<LightingProfile>(_entry);
        return _data;
    }

    // 🔹 Derived access
    public async Task<IReadOnlyList<LightingZone>> GetZonesAsync()
    {
        var data = await GetDataAsync();
        return data.Zones;
    }

    // 🔹 Runtime state (NOT from JSON)
    public bool IsPlaying => _player.IsPlaying(_entry.Id);

    public string Description { get; set; }
}

