using AmbinityDB.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var id = GenerateId(relativePath);
            var folder = Path.GetDirectoryName(file)!;
            var thumbPath = Path.GetRelativePath(_assetRoot,Path.Combine(folder, "thumbnail.png"));
            var entry = new ManifestEntry
            {
                Id = id,
                //if failed to read icon, use thumbnail instead
                Icon = TryReadIcon(file),
                Thumbnail = File.Exists(Path.GetFullPath(thumbPath,_assetRoot))?thumbPath :"",
                Name = new DirectoryInfo(Path.GetDirectoryName(file)).Name,
                Type = ResolveType(relativePath),
                Path = relativePath.Replace("\\", "/"),
                Size = info.Length,
                LastModified = info.LastWriteTimeUtc
            };

            manifest.Assets.Add(entry);
        }

        return manifest;
    }
    private string GenerateId(string relativePath)
    {
        var normalized = relativePath.Replace("\\", "/");

        // Remove file name → keep directory only
        var directory = Path.GetDirectoryName(normalized)?.Replace("\\", "/");

        if (string.IsNullOrEmpty(directory))
            return Path.GetFileNameWithoutExtension(normalized).ToLower();

        return directory
            .Replace("/", "-")
            .Replace(" ", "_")
            .ToLower();
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
    private string? TryReadIcon(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(reader);

            var jObject = JObject.Load(jsonReader);

            return jObject["Icon"]?.ToString();
        }
        catch
        {
            // ignore invalid JSON or missing field
            return null;
        }
    }
}
