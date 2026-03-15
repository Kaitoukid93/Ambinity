namespace AmbinityDB.Core.Models;

public class ManifestModel
{
    public int Version { get; set; } = 1;

    public List<ManifestEntry> Assets { get; set; } = new();
}
