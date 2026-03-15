using Newtonsoft.Json;

namespace AmbinityCore.Models.Collection;

/// <summary>
/// A lightweight descriptor stored in a repository manifest.
/// Contains the minimal data needed to display an item in UI and locate its full representation on disk.
/// </summary>
public class CollectableItemManifestEntry
{
    /// <summary>
    /// Unique identifier for this entry (e.g., file name without extension, GUID, or repository-specific key).
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Path to the file (or folder) that contains the full item definition.
    /// Usually relative to the repository's LocalFolderPath.
    /// </summary>
    [JsonProperty("localPath")]
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>
    /// The type identifier that the repository can use to resolve the concrete ICollectableItem implementation.
    /// </summary>
    [JsonProperty("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Optional metadata to display in lists without loading the full item.
    /// </summary>
    [JsonProperty("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
