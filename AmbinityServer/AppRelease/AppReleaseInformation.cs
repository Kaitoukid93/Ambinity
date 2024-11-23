using Newtonsoft.Json;

namespace AmbinityServer.AppRelease;

public class AppReleaseInformation
{
    public AppReleaseInformation(string version, DateTime releaseDate)
    {
        Version = version;
        ReleaseDate = releaseDate;
    }

    public AppReleaseInformation(string version)
    {
    }

    public AppReleaseInformation()
    {
    }

    public string Version { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string ChangeLog { get; set; }
    [JsonIgnore] public string Description => ReleaseDate + ": " + ChangeLog;
    [JsonIgnore] public string Path { get; set; }
}