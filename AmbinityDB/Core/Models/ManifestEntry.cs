namespace AmbinityDB.Core.Models;

public class ManifestEntry
{
    public string Id { get; set; } = default!;

    public string Type { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Path { get; set; } = default!;

    public string Source { get; set; } = "local";
    public string? Icon { get; set; }
    public string? Thumbnail { get; set; }
    public string? Category { get; set; }

    public string? Hash { get; set; }

    public long Size { get; set; }

    public DateTime? LastModified { get; set; }
}
