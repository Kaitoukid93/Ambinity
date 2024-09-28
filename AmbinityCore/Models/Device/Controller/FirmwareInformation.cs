
using Newtonsoft.Json;

namespace AmbinityCore.Models.Device.Controller;

public class FirmwareInformation
{
    public FirmwareInformation(string version, DateTime releaseDate)
    {
        
    }

    public FirmwareInformation(string version)
    {
        
    }

    public FirmwareInformation()
    {
        
    }
    public string Version { get; set; }
    public string TargetHardware { get; set; }
    public string Tool { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string ChangeLog { get; set; }
    [JsonIgnore]
    public string Path { get; set; }
}