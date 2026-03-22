
using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;
using AmbinityDB.Core.Services;
using AmbinityDB.Utils;

namespace AmbinityDB.Providers.Local;

public class LocalDatabaseSource : IDatabaseSource, IWritableDatabaseSource
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
    public async Task SaveManifestAsync(ManifestIndex index)
    {
        if (index == null)
            throw new ArgumentNullException(nameof(index));

        // 🔥 Only save entries that belong to THIS source
        var entries = index
            .All()
            .Where(e => e.Source == Source.Name)
            .ToList();

        var manifest = new ManifestModel
        {
            Assets = entries
        };

        // ensure directory exists
        var dir = Path.GetDirectoryName(_path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);

        // 🔥 write safely (overwrite)
        await using var stream = File.Create(_path);

        await JsonHelper.SerializeToFileAsync(stream, manifest);
    }
}
