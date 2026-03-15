using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class DatabaseManager
{
    private ManifestIndex _index = new();

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

    public IEnumerable<AssetMetadata> GetAssets(string type)
    {
        return _index
            .GetByType(type)
            .Select(x => new AssetMetadata
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Source = x.Source
            });
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
}
