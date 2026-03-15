using AmbinityDB.Core.Models;

namespace AmbinityDB.Core.Services;

public class ManifestValidator
{
    public bool Validate(
        ManifestModel manifest,
        string rootPath,
        out List<string> errors)
    {
        errors = new List<string>();

        var ids = new HashSet<string>();

        foreach (var entry in manifest.Assets)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
                errors.Add("Asset entry missing Id");

            if (string.IsNullOrWhiteSpace(entry.Type))
                errors.Add($"Asset {entry.Id} missing Type");

            if (string.IsNullOrWhiteSpace(entry.Path))
                errors.Add($"Asset {entry.Id} missing Path");

            if (!ids.Add(entry.Id))
                errors.Add($"Duplicate asset id: {entry.Id}");

            var filePath = Path.Combine(rootPath, entry.Path);

            if (!File.Exists(filePath))
                errors.Add($"File not found: {entry.Path}");
        }

        return errors.Count == 0;
    }
}
