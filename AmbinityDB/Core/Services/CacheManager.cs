using AmbinityDB.Core.Interfaces;

namespace AmbinityDB.Core.Services;

public class CacheManager : IAssetCache
{
    private readonly Dictionary<string, object> _cache = new();

    public bool TryGet(string key, out object? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Store(string key, object value)
    {
        _cache[key] = value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
