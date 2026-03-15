using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class AssetResolver : IAssetResolver
{
    private readonly Dictionary<string, IDatabaseSource> _sources;
    private readonly HttpClient _http = new();

    public AssetResolver(IEnumerable<IDatabaseSource> sources)
    {
        _sources = sources.ToDictionary(x => x.Source.Name);
    }

    public async Task<Stream> ResolveAsync(ManifestEntry entry)
    {
        if (!_sources.TryGetValue(entry.Source, out var source))
            throw new InvalidOperationException($"Unknown source {entry.Source}");

        if (source.Source.IsRemote)
        {
            var url = $"{source.Source.BasePath.TrimEnd('/')}/{entry.Path}";
            return await _http.GetStreamAsync(url);
        }

        var path = Path.Combine(source.Source.BasePath, entry.Path);

        return File.OpenRead(path);
    }
}
