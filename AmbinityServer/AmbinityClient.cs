using AmbinityServer.AppRelease;
using Serilog;

namespace AmbinityServer;

public class AmbinityClient
{

    private SftpWrapper _ftpServer;
    public SftpWrapper SftpServer => _ftpServer;

    private GitHubReleaseClient _gitHubClient;
    public GitHubReleaseClient GitHubClient => _gitHubClient;

    public string HomeAddress = "/home/adrilight_enduser/";

    /// <summary>
    /// Initialize with GitHub release download support
    /// </summary>
    /// <param name="gitHubOwner">GitHub repository owner (e.g., "Ambino")</param>
    /// <param name="gitHubRepo">GitHub repository name (e.g., "Ambinity")</param>
    /// <param name="assetName">Asset name to download (default: "Ambinity.zip")</param>
    public AmbinityClient(string gitHubOwner = "", string gitHubRepo = "", string assetName = "Ambinity.zip")
    {
        _ftpServer = new SftpWrapper("adrilight_publicuser","@drilightPublic");

        // Initialize GitHub client if owner and repo are provided
        if (!string.IsNullOrWhiteSpace(gitHubOwner) && !string.IsNullOrWhiteSpace(gitHubRepo))
        {
            _gitHubClient = new GitHubReleaseClient(gitHubOwner, gitHubRepo, assetName);
        }
    }

    public async Task<bool> Init()
    {
        return await _ftpServer.Connect();
    }


    public void Disconnect()
    {
        Log.Information("Disposing connection to Ambinity server...");
        _ftpServer.Disconnect();
        Log.Information("Ambinity server connection is disposed");
    }
}
