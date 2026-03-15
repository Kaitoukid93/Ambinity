using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmbinityCore.Models.Manifest;
public sealed class DeviceFirmwareModel
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("group")]
    public string Group { get; set; } = string.Empty;

    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("hardware")]
    public List<DeviceHardwareModel> Hardware { get; set; } = new();
}
