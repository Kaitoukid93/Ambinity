using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace AmbinityServer.AppRelease;

/// <summary>
/// Client for downloading releases from GitHub
/// </summary>
public class GitHubReleaseClient
{
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _assetName;
    private readonly HttpClient _httpClient;

    public GitHubReleaseClient(string owner, string repo, string assetName = "Ambinity.zip")
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _assetName = assetName;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ambinity-Installer");
    }

    /// <summary>
    /// Get all available releases from GitHub
    /// </summary>
    public async Task<List<AppReleaseInformation>> GetAvailableReleases()
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{_owner}/{_repo}/releases";
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(content);

            if (releases == null || releases.Count == 0)
            {
                Log.Information("No releases found on GitHub");
                return new List<AppReleaseInformation>();
            }

            var result = new List<AppReleaseInformation>();
            foreach (var release in releases)
            {
                // Find the Ambinity.zip asset
                var asset = release.Assets?.FirstOrDefault(a => a.Name.Equals(_assetName, StringComparison.OrdinalIgnoreCase));
                if (asset != null)
                {
                    var appRelease = new AppReleaseInformation(
                        version: release.TagName ?? release.Name,
                        releaseDate: release.PublishedAt ?? release.CreatedAt
                    )
                    {
                        ChangeLog = release.Body ?? "No changelog available",
                        Path = asset.BrowserDownloadUrl
                    };
                    result.Add(appRelease);
                }
            }

            if (result.Count == 0)
            {
                Log.Warning($"No releases found with asset '{_assetName}'");
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch releases from GitHub");
            return new List<AppReleaseInformation>();
        }
    }

    /// <summary>
    /// Download a specific release asset
    /// </summary>
    public async Task<bool> DownloadAsset(string downloadUrl, string outputPath, IProgress<int> progress)
    {
        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl))
            {
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var fileStream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None, 8192, true))
                        {
                            var totalRead = 0L;
                            var buffer = new byte[8192];
                            int read;

                            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (canReportProgress)
                                {
                                    var progressPercentage = (int)((totalRead * 100) / totalBytes);
                                    progress?.Report(progressPercentage);
                                }
                            }
                        }
                    }
                }
            }

            progress?.Report(100);
            Log.Information($"Successfully downloaded asset to {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download asset from GitHub");
            return false;
        }
    }

    private class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonProperty("assets")]
        public List<GitHubAsset> Assets { get; set; }
    }

    private class GitHubAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}
