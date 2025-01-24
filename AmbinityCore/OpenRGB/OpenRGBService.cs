using System.Diagnostics;
using System.IO.Compression;
using AmbinityCore.Utils;
using AmbinityServer;
using Serilog;

namespace AmbinityCore.OpenRGB;

/// <summary>
/// provide some calls to mânge openrgb process
/// </summary>
public class OpenRGBService
{
    private string OpenRGBExcutablePath =
        Path.Combine(Constants.AppDataFolder, "OpenRGB", "OpenRGB Windows 64-bit", "OpenRGB.exe");
    
    private string OpenRGBDownloadablePath => _client.HomeAddress+ "ftp/files/OpenRGB/OpenRGB_Windows_64_7085106b.zip";

    private readonly AmbinityClient _client;

    public OpenRGBService(AmbinityClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Start openRGB in ambinity folder
    /// </summary>
    public async Task StartOpenRGBProcess()
    {
        if (!File.Exists(OpenRGBExcutablePath))
        {
            var downloadedOpenRGBArchive = await DownloadOpenRGB();
            if (!File.Exists(downloadedOpenRGBArchive))
            {
                Log.Error("OpenRGB archive not found or server is not available, please re-download");
                return;
            }
            ExtractOpenRGB(downloadedOpenRGBArchive);
        }
            //download open rgb if not exist
            Log.Error("OpenRGB not found, please Reinstall application");
        if (isRunning("OpenRGB"))
            Log.Information("OpenRGB is already running");
        await ProcessUtilities.RunProcessAsync(OpenRGBExcutablePath, "--server --startminimized --gui");
    }
    private async Task ExtractOpenRGB(string file, IProgress<int> progress =null)
    {
        //clear the installing path first
        if (Directory.Exists(Path.Combine(Constants.AppDataFolder, "OpenRGB")))
            Directory.Delete(Path.Combine(Constants.AppDataFolder, "OpenRGB"), true);
        else
        {
            Directory.CreateDirectory(Path.Combine(Constants.AppDataFolder, "OpenRGB"));
        }
        await using FileStream fileStream = new FileStream(file, FileMode.Open);
        ZipArchive archive = new ZipArchive(fileStream);
        archive.ExtractToDirectory(Path.Combine(Constants.AppDataFolder, "OpenRGB"));
        
    }
   
    private async Task<string> DownloadOpenRGB()
    {
        var result = await _client.Init();
        if (!result)
        {
            return (null);
        }
        //create cache folder
        if (Directory.Exists(Constants.CacheFolderPath))
            Directory.Delete(Constants.CacheFolderPath, true);
        else
        {
            Directory.CreateDirectory(Constants.CacheFolderPath);
        }

        await Task.Run(() => _client.SftpServer.DownloadFile(OpenRGBDownloadablePath, Path.Combine(Constants.CacheFolderPath,"OpenRGB.zip")));
        return Path.Combine(Constants.CacheFolderPath, "OpenRGB.zip");
    }

    private bool isRunning(string name)
    {
        try
        {
            var processes = Process.GetProcessesByName(name);
            if (processes.Count() == 0 || processes == null)
                return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }
}