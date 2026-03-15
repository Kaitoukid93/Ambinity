using Newtonsoft.Json;

namespace AmbinityCore.Models.Manifest;
public sealed class ManifestModel
{
    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonProperty("app")]
    public string App { get; set; } = string.Empty;

    [JsonProperty("generatedAt")]
    public DateTime GeneratedAt { get; set; }

    [JsonProperty("categories")]
    public IReadOnlyList<CategoryModel> Categories { get; set; } = [];
}
