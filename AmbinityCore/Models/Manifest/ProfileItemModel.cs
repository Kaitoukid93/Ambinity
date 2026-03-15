using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmbinityCore.Models.Manifest;
public sealed class ProfileItemModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("author")]
    public string Author { get; set; } = string.Empty;

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("entry")]
    public string Entry { get; set; } = string.Empty;

    [JsonProperty("thumb")]
    public string Thumb { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("screenshotsPath")]
    public string ScreenshotsPath { get; set; } = string.Empty;
}
