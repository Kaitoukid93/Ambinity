namespace AmbinityServer.Http;
public sealed class HttpContentClient : IContentClient, IDisposable
{
    private readonly HttpClient _http;

    public HttpContentClient()
    {
        _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Required for GitHub
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Ambinity/1.0");
    }

    public async Task<string> GetTextAsync(string url, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<byte[]> GetBytesAsync(string url, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task DownloadFileAsync(string url, string localPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var httpStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(localPath);

        await httpStream.CopyToAsync(fileStream, ct);
    }

    public void Dispose()
    {
        _http.Dispose();
    }
}

