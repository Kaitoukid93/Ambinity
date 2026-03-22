using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class DatabaseManager
{
    private ManifestIndex _index = new();
    public event Action<ManifestEntry>? AssetAdded;
    public event Action<string>? AssetRemoved;
    public event Action<ManifestEntry>? AssetUpdated;
    private readonly IAssetResolver _resolver;

    private readonly IEnumerable<IDatabaseSource> _sources;

    public DatabaseManager(
        IEnumerable<IDatabaseSource> sources,
        IAssetResolver resolver)
    {
        _sources = sources;
        _resolver = resolver;
    }

    public async Task InitializeAsync()
    {
        foreach (var source in _sources)
        {
            var manifest = await source.LoadManifestAsync();

            foreach (var entry in manifest.Assets)
            {
                entry.Source = source.Source.Name;

                _index.Add(entry);
            }
        }
    }

    public IEnumerable<ManifestEntry> GetEntries(string type)
    {
        return _index.GetByType(type);
    }
    public ManifestEntry? GetEntry(string id)
    {
        return _index.Get(id);
    }

    public async Task<Stream> LoadAssetAsync(string id)
    {
        var entry = _index.Get(id)
            ?? throw new KeyNotFoundException($"Asset {id} not found");

        return await _resolver.ResolveAsync(entry);
    }
    public void SetIndex(ManifestIndex index)
    {
        _index = index ?? throw new ArgumentNullException(nameof(index));
    }
    public async Task AddAsync(ManifestEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        // avoid duplicates
        if (_index.Get(entry.Id) != null)
            throw new InvalidOperationException($"Asset {entry.Id} already exists");

        _index.Add(entry);

        // persist if writable
        var source = _sources.FirstOrDefault(s => s.Source.Name == entry.Source);

        if (source is IWritableDatabaseSource writable && !source.Source.IsRemote)
        {
            await writable.SaveManifestAsync(_index);
        }

        AssetAdded?.Invoke(entry);
    }
    public async Task<bool> RemoveAsync(string id)
    {
        var entry = _index.Get(id);

        if (entry == null)
            return false;

        _index.Remove(id);

        var source = _sources.FirstOrDefault(s => s.Source.Name == entry.Source);

        if (source is IWritableDatabaseSource writable && !source.Source.IsRemote)
        {
            await writable.SaveManifestAsync(_index);
        }

        AssetRemoved?.Invoke(id);

        return true;
    }
    public async Task UpdateAsync(ManifestEntry entry)
    {
        _index.Add(entry); // 🔥 upsert

        var source = _sources.FirstOrDefault(s => s.Source.Name == entry.Source);

        if (source is IWritableDatabaseSource writable && !source.Source.IsRemote)
        {
            await writable.SaveManifestAsync(_index);
        }

        AssetUpdated?.Invoke(entry);
    }

}
