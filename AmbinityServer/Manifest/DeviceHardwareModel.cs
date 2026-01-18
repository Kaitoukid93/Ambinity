using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmbinityServer.Manifest;
public sealed class DeviceHardwareModel
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("firmwares")]
    public List<FirmwareVersionModel> Firmwares { get; set; } = new();
}
