using Newtonsoft.Json;

namespace AmbinityCore.Models.Manifest;
public sealed class FirmwareVersionModel
{
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("binary")]
    public string Binary { get; set; } = string.Empty;

    [JsonProperty("info")]
    public string Info { get; set; } = string.Empty;
}
