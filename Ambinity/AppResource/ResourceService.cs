using System;
using System.IO;
using System.Threading.Tasks;
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
    public const string ImageRemotePath = "/home/adrilight_developeruser/ftp/files/Resources/Thumbs";
    public const string DeviceRemotePath = "/home/adrilight_developeruser/ftp/files/Resources/AmbinityDevices";

    public ResourceService(DownloadService downloadService)
    {
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