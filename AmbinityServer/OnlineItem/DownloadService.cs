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
    /// <summary>
    /// download first item from directory with specific name
    /// </summary>
    /// <param name="remotePath"></param>
    /// <param name="localPath"></param>
    /// <param name="progress"></param>
    public async Task<string> DownloadItemWithName(string directory,string name, string localDirectory, IProgress<DownloadProgress> progress)
    {
        IsDownloading = true;
        if (!_isInit)
        {
            var result = await Init();
            if(!result)
                return null;
        }

        var file = await _sftpServer.GetFileByNameMatching(name, directory);
        if (file != null)
        {
             _sftpServer.DownloadFile(file.FullName, Path.Combine(localDirectory,file.Name));
             return Path.Combine(localDirectory, file.Name);
        }

        return null;
    }
    /// <summary>
    /// get item screenshot
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    // public async Task<List<BitmapImage>> GetItemScreenShots(OnlineItemModel item)
    // {
    //     var screenShots = new List<BitmapImage>();
    //     var screenshotsPath = item.Path + "/screenshots";
    //     foreach (var file in _ftpServer.GetAllFilesAddressInFolder(screenshotsPath).Result)
    //     {
    //         var img = await _ftpServer.GetScreenShot(file);
    //         screenShots.Add(img);
    //     }
    //     return screenShots;
    // }
    /// <summary>
    /// get item markdown description
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<string> GetItemDescription(OnlineItem item)
    {
        var descriptionPath = item.Path + "/description.md";
        var description = await _sftpServer.GetStringContent(descriptionPath);
        return description;
    }
}