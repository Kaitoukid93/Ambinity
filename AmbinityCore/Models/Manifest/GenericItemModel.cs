using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmbinityCore.Models.Manifest;
public sealed class GenericItemModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("files")]
    public List<string> Files { get; set; } = new();
}
