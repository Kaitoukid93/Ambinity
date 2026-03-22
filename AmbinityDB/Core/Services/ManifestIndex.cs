using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class ManifestIndex
{
    private readonly Dictionary<string, ManifestEntry> _byId = new();

    private readonly Dictionary<string, List<ManifestEntry>> _byType = new();

    // =========================================================
    // ADD (UPSERT SAFE)
    // =========================================================
    public void Add(ManifestEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        // 🔥 If exists → remove old first (prevent duplicates)
        if (_byId.TryGetValue(entry.Id, out var existing))
        {
            RemoveFromTypeIndex(existing);
        }

        // add to id index
        _byId[entry.Id] = entry;

        // add to type index
        if (!_byType.TryGetValue(entry.Type, out var list))
        {
            list = new List<ManifestEntry>();
            _byType[entry.Type] = list;
        }

        list.Add(entry);
    }

    // =========================================================
    // REMOVE
    // =========================================================
    public bool Remove(string id)
    {
        if (!_byId.TryGetValue(id, out var entry))
            return false;

        // remove from type index
        RemoveFromTypeIndex(entry);

        // remove from id index
        _byId.Remove(id);

        return true;
    }

    // =========================================================
    // INTERNAL HELPERS
    // =========================================================
    private void RemoveFromTypeIndex(ManifestEntry entry)
    {
        if (!_byType.TryGetValue(entry.Type, out var list))
            return;

        list.RemoveAll(e => e.Id == entry.Id);

        // optional cleanup (nice to have)
        if (list.Count == 0)
            _byType.Remove(entry.Type);
    }

    // =========================================================
    // QUERY
    // =========================================================
    public ManifestEntry? Get(string id)
    {
        _byId.TryGetValue(id, out var entry);
        return entry;
    }

    public IEnumerable<ManifestEntry> GetByType(string type)
    {
        return _byType.TryGetValue(type, out var list)
            ? list
            : Enumerable.Empty<ManifestEntry>();
    }

    public IEnumerable<ManifestEntry> All()
    {
        return _byId.Values;
    }
}
