namespace AmbinityServer.Http;

public interface IContentClient
{
    Task<string> GetTextAsync(string url, CancellationToken ct = default);
    Task<byte[]> GetBytesAsync(string url, CancellationToken ct = default);
    Task DownloadFileAsync(string url, string localPath, CancellationToken ct = default);
}
