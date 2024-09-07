using AmbinityServer.Download;
using Serilog;

namespace AmbinityServer.OnlineItem;

/// <summary>
/// service to download item 
/// </summary>
public class DownloadService
{
    private readonly AmbinityClient _ambinityClient;
    private readonly SftpWrapper _sftpServer;
    private bool _isInit;
    public bool IsDownloading { get; set; }
    public DownloadService(AmbinityClient ambinityClient)
    {
        _ambinityClient = ambinityClient;
        _sftpServer = _ambinityClient.SftpServer;
    }

    private async Task<bool> Init()
    {
        var result = await _ambinityClient.Init();
        if (!result)
        {
            Log.Error("Server or network is not available");
            _isInit = false;
            return false;
        }
        _isInit = true;
        return true;
        
    }
    /// <summary>
    /// download an OnlineItem to destination path
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destination"></param>
    /// <param name="progress"></param>
    public async Task DownloadItem(OnlineItem item, string destination, IProgress<DownloadProgress> progress)
    {
        IsDownloading = true;
        if (!_isInit)
        {
           var result = await Init();
           if(!result)
               return;
        }
        await _sftpServer.DownloadDirectory(item.Path + "/content", destination, progress);
        IsDownloading = false;
    }

    /// <summary>
    /// download remote directory to destination, make sure destination is exists and available to write
    /// </summary>
    /// <param name="remotePath"></param>
    /// <param name="localPath"></param>
    /// <param name="progress"></param>
    public async Task DownloadDirectory(string remotePath, string localPath, IProgress<DownloadProgress> progress)
    {
        IsDownloading = true;
        if (!_isInit)
        {
            var result = await Init();
            if(!result)
                return;
        }
        await _sftpServer.DownloadDirectory(remotePath, localPath, progress);
    }
}