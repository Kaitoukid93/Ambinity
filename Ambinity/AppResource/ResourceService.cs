using System;
using System.IO;
using System.Threading.Tasks;
using AmbinityServer;
using AmbinityServer.Download;
using AmbinityServer.OnlineItem;
using Serilog.Core;
using Constants = AmbinityCore.Constants;

namespace Ambinity.AppResource;

/// <summary>
///  manage first run resource
/// </summary>
public class ResourceService
{
    private string ImagesLocalFolderPath => Path.Combine(Constants.AppDataFolder, "Images");
    private string AmbinityDeviceFolderPath => Path.Combine(Constants.AppDataFolder, "AmbinityDevices");
    public string ImageRemotePath;
    public string DeviceRemotePath;

    public ResourceService(AmbinityClient client, DownloadService downloadService)
    {
        ImageRemotePath = client.HomeAddress + "ftp/files/Resources/Thumbs";
        DeviceRemotePath = client.HomeAddress + "ftp/files/Resources/AmbinityDevices";
        _downloadService = downloadService;
    }

    private DownloadService _downloadService;

    /// <summary>
    /// download first run resource from server
    /// </summary>
    public async Task DownloadFirstRunResource(IProgress<DownloadProgress> progress)
    {
        //download device
        if (!Directory.Exists(AmbinityDeviceFolderPath))
            await _downloadService.DownloadDirectory(DeviceRemotePath, AmbinityDeviceFolderPath, progress);
        //download image
        if (!Directory.Exists(ImagesLocalFolderPath))
            await _downloadService.DownloadDirectory(ImageRemotePath, ImagesLocalFolderPath, progress);
        //download profiles
    }
}