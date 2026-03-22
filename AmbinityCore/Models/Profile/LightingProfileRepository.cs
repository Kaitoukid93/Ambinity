using System.Collections.ObjectModel;
using System.Linq;
using AmbinityCore.Models.Profile;
using AmbinityCore.Models.Profile.Service;
using AmbinityCore.Repositories.Loader;
using AmbinityDB.Core.Models;
using AmbinityDB.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Repositories;

public class LightingProfileRepository : ObservableObject
{
    private readonly DatabaseManager _db;
    private readonly IAssetLoader _loader;
    private readonly IPlayerService _player;

    public ObservableCollection<LightingProfileItem> Items { get; } = new();

    // 🔥 optional fast lookup
    private readonly Dictionary<string, LightingProfileItem> _map = new();

    public LightingProfileRepository(
        DatabaseManager db,
        IAssetLoader loader,
        IPlayerService player)
    {
        _db = db;
        _loader = loader;
        _player = player;

        _db.AssetAdded += OnAssetAdded;
        _db.AssetRemoved += OnAssetRemoved;
        _db.AssetUpdated += OnAssetUpdated;
    }

    // =========================================================
    // INIT
    // =========================================================
    public void Initialize()
    {
        Items.Clear();
        _map.Clear();

        var entries = _db.GetEntries(AssetTypes.Profile);

        foreach (var entry in entries)
        {
            var item = CreateItem(entry);
            AddInternal(item);
        }
    }

    // =========================================================
    // CREATE ITEM
    // =========================================================
    private LightingProfileItem CreateItem(ManifestEntry entry)
    {
        return new LightingProfileItem(entry, _loader, _player);
    }

    // =========================================================
    // INTERNAL ADD
    // =========================================================
    private void AddInternal(LightingProfileItem item)
    {
        _map[item.Id] = item;
        Items.Add(item);
    }

    // =========================================================
    // DB EVENTS
    // =========================================================
    private void OnAssetAdded(ManifestEntry entry)
    {
        if (entry.Type != AssetTypes.Profile)
            return;

        var item = CreateItem(entry);
        AddInternal(item);
    }

    private void OnAssetUpdated(ManifestEntry entry)
    {
        if (!_map.TryGetValue(entry.Id, out var item))
            return;

        item.Update(entry); // 🔥 update item
    }
    private void OnAssetRemoved(string id)
    {
        if (!_map.TryGetValue(id, out var item))
            return;

        _map.Remove(id);
        Items.Remove(item);
    }

    // =========================================================
    // QUERY HELPERS
    // =========================================================
    public LightingProfileItem? GetById(string id)
    {
        return _map.TryGetValue(id, out var item) ? item : null;
    }

    // public IEnumerable<LightingProfileItem> GetPinned()
    // {
    //     return Items.Where(x => x.IsPinned);
    // }

    // =========================================================
    // OPTIONAL SORT
    // =========================================================
    public void SortByName()
    {
        var sorted = Items.OrderBy(x => x.Name).ToList();

        Items.Clear();
        foreach (var item in sorted)
            Items.Add(item);
    }
}
