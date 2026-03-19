using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class GitHubResourceSource : IResourceSource
{
    private readonly string _url;
    private readonly HttpClient _httpClient;

    public GitHubResourceSource(string url, HttpClient? httpClient = null)
    {

    }

    public async Task<Stream> GetArchiveAsync(CancellationToken cancellationToken = default)
    {
       return null;
    }
}
