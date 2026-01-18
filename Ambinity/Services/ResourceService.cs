using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using AmbinityServer;
using AmbinityServer.Download;
using AmbinityServer.OnlineItem;
using Serilog;
using Serilog.Core;
using Constants = AmbinityCore.Constants;

namespace Ambinity.Services;

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
    /// <summary>
    /// copy first run resource from embedded resources
    /// </summary>
    public async Task CopyFirstRunResource(IProgress<DownloadProgress> progress)
    {
        // Copy language packs
        if (!Directory.Exists(AmbinityLocalesFolderPath))
        {
            Directory.CreateDirectory(AmbinityLocalesFolderPath);
            // Assuming the embedded resources are accessible via a method to extract them
            ExtractEmbeddedResource("Ambinity.AppResource.Locales", AmbinityLocalesFolderPath);
        }

        // Copy device
        if (!Directory.Exists(AmbinityDeviceFolderPath))
        {
            Directory.CreateDirectory(AmbinityDeviceFolderPath);
            ExtractEmbeddedResource("Ambinity.AppResource.AmbinityDevices", AmbinityDeviceFolderPath);
            //extract zip if needed
            if(File.Exists(Path.Combine(AmbinityDeviceFolderPath, "DefaultDevices.zip")))
            ZipFile.ExtractToDirectory(Path.Combine(AmbinityDeviceFolderPath, "DefaultDevices.zip"), AmbinityDeviceFolderPath, true);
            File.Delete(Path.Combine(AmbinityDeviceFolderPath, "DefaultDevices.zip"));
             // Remove __MACOSX folder if it exists
            string macOSXFolder = Path.Combine(AmbinityDeviceFolderPath, "__MACOSX");
            if (Directory.Exists(macOSXFolder))
            {
                Directory.Delete(macOSXFolder, true);
            }
        }

        // Copy image
        if (!Directory.Exists(ImagesLocalFolderPath))
        {
            Directory.CreateDirectory(ImagesLocalFolderPath);
            ExtractEmbeddedResource("Ambinity.AppResource.Images", ImagesLocalFolderPath);
        }

        // Copy profiles
        if (!Directory.Exists(ProfileFolderPath))
        {
            Directory.CreateDirectory(ProfileFolderPath);
            ExtractEmbeddedResource("Ambinity.AppResource.AmbinityLightingProfiles", ProfileFolderPath);
        }

        // Copy tools
    }

    /// <summary>
    /// Extracts embedded resources from the assembly to the specified destination folder
    /// </summary>
    private void ExtractEmbeddedResource(string resourcePrefix, string destinationFolder)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (resourceName.StartsWith(resourcePrefix))
                {
                    // Extract the relative path from the resource name
                    string relativePath = resourceName.Substring(resourcePrefix.Length + 1); // +1 to skip the dot
                    string destinationPath = Path.Combine(destinationFolder, relativePath);

                    // Create directory if it doesn't exist
                    string destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Extract the resource and write to file
                    using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream != null)
                        {
                            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                            {
                                resourceStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error extracting embedded resource from {resourcePrefix} to {destinationFolder}");
        }
    }
}
