
using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;
using AmbinityDB.Utils;

namespace AmbinityDB.Providers.Local;

public class LocalDatabaseSource : IDatabaseSource
{
    public DatabaseSource Source { get; }

    private readonly string _path;

    public LocalDatabaseSource(string name, string basePath)
    {
        Source = new DatabaseSource
        {
            Name = name,
            BasePath = basePath,
            IsRemote = false
        };

        _path = Path.Combine(basePath, "manifest.json");
    }

    public async Task<ManifestModel> LoadManifestAsync()
    {
        if (!File.Exists(_path))
            throw new FileNotFoundException($"Manifest not found: {_path}");

        await using var stream = File.OpenRead(_path);

        var manifest = await JsonHelper.DeserializeAsync<ManifestModel>(stream)
                      ?? new ManifestModel();

        return manifest;
    }
}
