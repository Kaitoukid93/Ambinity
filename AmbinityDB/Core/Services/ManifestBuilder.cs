using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class ManifestBuilder
{
    private readonly string _assetRoot;

    public ManifestBuilder(string assetRoot)
    {
        _assetRoot = assetRoot;
    }

    public ManifestModel Build()
    {
        var manifest = new ManifestModel();

        var files = Directory.GetFiles(
            _assetRoot,
            "*.json",
            SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (Path.GetFileName(file).Equals("manifest.json",
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var relativePath = Path.GetRelativePath(_assetRoot, file);

            var info = new FileInfo(file);

            var entry = new ManifestEntry
            {
                Id = Path.GetFileNameWithoutExtension(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Type = ResolveType(relativePath),
                Path = relativePath.Replace("\\", "/"),
                Size = info.Length,
                LastModified = info.LastWriteTimeUtc
            };

            manifest.Assets.Add(entry);
        }

        return manifest;
    }

    private string ResolveType(string relativePath)
    {
        var parts = relativePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        return parts.Length > 0
            ? parts[0]
            : "unknown";
    }
}
