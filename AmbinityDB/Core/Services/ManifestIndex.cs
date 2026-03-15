using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class ManifestIndex
{
    private readonly Dictionary<string, ManifestEntry> _byId = new();

    private readonly Dictionary<string, List<ManifestEntry>> _byType = new();

    public void Add(ManifestEntry entry)
    {
        _byId[entry.Id] = entry;

        if (!_byType.TryGetValue(entry.Type, out var list))
        {
            list = new List<ManifestEntry>();
            _byType[entry.Type] = list;
        }

        list.Add(entry);
    }

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
