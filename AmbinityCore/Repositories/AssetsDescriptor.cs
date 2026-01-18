namespace AmbinityCore.Repositories;

public sealed class AssetDescriptor
{
    // Identity
    public string Id { get; set; } = string.Empty;

    // Classification
    public string AssetType { get; set; } = string.Empty; // Profile, Device, Palette
    public List<string> Tags { get; set; } = new();

    // Display
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty; // relative path or URL

    // Location
    public string RelativePath { get; set; } = string.Empty;
    public AssetSource Source { get; set; }

    // Capabilities (optional but very useful)
    public bool CanDownload { get; set; }
    public bool CanDelete { get; set; }
    public bool CanEdit { get; set; }

}


public enum AssetSource
{
    Local,
    Online
}
