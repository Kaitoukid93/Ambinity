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
    private string AmbinityLocalesFolderPath => Path.Combine(Constants.AppDataFolder, "Locales");
    private string ProfileFolderPath => Path.Combine(Constants.ModelDataFolder, "Profiles");
    private string ImageRemotePath;
    private string DeviceRemotePath;
    private string ProfileRemotePath;
    private string FirmwareToolsRemotePath;
    private string LocalesRemotePath;

    public ResourceService(AmbinityClient client, DownloadService downloadService)
    {
        _downloadService = downloadService;
        ImageRemotePath = client.HomeAddress + "ftp/files/Resources/Thumbs";
        DeviceRemotePath = client.HomeAddress + "ftp/files/Resources/AmbinityDevices";
        ProfileRemotePath = client.HomeAddress + "ftp/files/Resources/LightingProfiles";
        FirmwareToolsRemotePath = client.HomeAddress + "/ftp/files/Firmwares/Tools";
        LocalesRemotePath = client.HomeAddress + "ftp/files/Resources/Locales";
    }

    private DownloadService _downloadService;

    /// <summary>
    /// download first run resource from server
    /// </summary>
    public async Task DownloadFirstRunResource(IProgress<DownloadProgress> progress)
    {
        //download language packs
        if (!Directory.Exists(AmbinityLocalesFolderPath))
            await _downloadService.DownloadDirectory(LocalesRemotePath, AmbinityLocalesFolderPath, progress);
        //download device
        if (!Directory.Exists(AmbinityDeviceFolderPath))
            await _downloadService.DownloadDirectory(DeviceRemotePath, AmbinityDeviceFolderPath, progress);
        //download image
        if (!Directory.Exists(ImagesLocalFolderPath))
            await _downloadService.DownloadDirectory(ImageRemotePath, ImagesLocalFolderPath, progress);
        //download profiles
        //clear cache
        if (!Directory.Exists(ProfileFolderPath))
            await _downloadService.DownloadDirectory(ProfileRemotePath, ProfileFolderPath, progress);
        //download tools
        if (!Directory.Exists(Constants.FirmwareToolsFolderPath))
        {
            //download from server
            Directory.CreateDirectory(Constants.FirmwareToolsFolderPath);
            await _downloadService.DownloadDirectory(FirmwareToolsRemotePath, Constants.FirmwareToolsFolderPath, null);
        }
    }
}
