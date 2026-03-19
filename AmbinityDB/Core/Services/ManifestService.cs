using AmbinityDB.Core.Models;
using AmbinityDB.Utils;

namespace AmbinityDB.Core.Services;
public class ManifestService
{
    private readonly ManifestIndex _index;
    private readonly string _path;

    public ManifestService(string basePath, ManifestIndex index)
    {
        _index = index;
        _path = Path.Combine(basePath, "manifest.json");
    }

    public async Task SaveAsync()
    {
        var model = new ManifestModel
        {
            Assets = _index.All().ToList()
        };

        await using var stream = File.Create(_path);

        await JsonHelper.SerializeToFileAsync(stream, model);
    }
}
