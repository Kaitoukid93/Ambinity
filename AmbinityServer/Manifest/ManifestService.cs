using AmbinityServer.Http;
using Newtonsoft.Json;

namespace AmbinityServer.Manifest;
public sealed class ManifestService
{
    private readonly IContentClient _client;
    private readonly string _manifestUrl;

    public ManifestService(IContentClient client, string manifestUrl)
    {
        _client = client;
        _manifestUrl = manifestUrl;
    }

    public async Task<ManifestModel> LoadAsync(CancellationToken ct = default)
    {
        var json = await _client.GetTextAsync(_manifestUrl, ct);
        return JsonConvert.DeserializeObject<ManifestModel>(json)!;
    }
}

