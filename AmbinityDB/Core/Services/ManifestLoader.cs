using Newtonsoft.Json;
using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class ManifestLoader
{
    private readonly string _manifestPath;

    public ManifestLoader(string manifestPath)
    {
        _manifestPath = manifestPath;
    }

    public async Task<ManifestModel?> LoadAsync()
    {
        if (!File.Exists(_manifestPath))
            return null;

        var json = await File.ReadAllTextAsync(_manifestPath);

        return JsonConvert.DeserializeObject<ManifestModel>(json);
    }

    public async Task SaveAsync(ManifestModel manifest)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath)!);

        var json = JsonConvert.SerializeObject(
            manifest,
            Formatting.Indented);

        await File.WriteAllTextAsync(_manifestPath, json);
    }
}
