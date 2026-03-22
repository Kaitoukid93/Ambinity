
using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;
using AmbinityDB.Utils;

namespace AmbinityCore.Repositories.Loader;

class AssetLoader : IAssetLoader
{
    private readonly IAssetResolver _resolver;

    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, Task<object>> _inflight = new();

    public async Task<T> LoadAsync<T>(ManifestEntry entry)
    {
        var key = $"{entry.Source}:{entry.Path}:{typeof(T)}";

        if (_cache.TryGetValue(key, out var cached))
            return (T)cached;

        if (_inflight.TryGetValue(key, out var task))
            return (T)await task;

        var loadTask = LoadInternal<T>(entry);
        _inflight[key] = loadTask;

        var result = await loadTask;

        _inflight.Remove(key);
        _cache[key] = result;

        return (T)result;
    }

    private async Task<object> LoadInternal<T>(ManifestEntry entry)
    {
        using var stream = await _resolver.ResolveAsync(entry);
        return JsonHelper.DeserializeAsync<T>(stream);
    }
}
