using AmbinityDB.Core.Interfaces;
using AmbinityDB.Core.Models;
using AmbinityDB.Utils;

namespace AmbinityDB.Providers.Remote;

public class RemoteDatabaseSource : IDatabaseSource
{
    public DatabaseSource Source { get; }

    private readonly HttpClient _http;
    private readonly string _manifestUrl;

    public RemoteDatabaseSource(
        string name,
        string baseUrl,
        HttpClient httpClient)
    {
        Source = new DatabaseSource
        {
            Name = name,
            BasePath = baseUrl,
            IsRemote = true
        };

        _manifestUrl = $"{baseUrl.TrimEnd('/')}/manifest.json";
        _http = httpClient;
    }

    public async Task<ManifestModel> LoadManifestAsync()
    {
        using var stream = await _http.GetStreamAsync(_manifestUrl);

        var manifest = await JsonHelper.DeserializeAsync<ManifestModel>(stream)
                      ?? new ManifestModel();

        return manifest;
    }
}
