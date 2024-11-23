using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Ambinity.Utils;
using AmbinityCore;
using AmbinityServer;
using AmbinityServer.AppRelease;
using Serilog;

namespace Ambinity.Services;

public class UpdateService
{
    private readonly AmbinityClient _client;
    private readonly string _releaseRemotePath;

    public UpdateService(AmbinityClient client)
    {
        _client = client;
        _releaseRemotePath = _client.HomeAddress + "ftp/files/AppRelease";
        _client = client;
    }

    /// <summary>
    /// Download latest release
    /// </summary>
    /// <param name="progress"></param>
    public async Task<(string, AppReleaseInformation)> DownloadRelease(IProgress<int> progress,
        AppReleaseInformation info = null)
    {
        var availableRelease = await GetAvailableRelease();
        if (availableRelease == null || availableRelease.Count == 0)
            return (null, null);
        var selectedRelease = info == null ? availableRelease.OrderBy(r => r.ReleaseDate).First() : info;
        //download zip
        if (!Directory.Exists(Constants.CacheFolderPath))
            Directory.CreateDirectory(Constants.CacheFolderPath);
        var downloadPath = Path.Combine(Constants.CacheFolderPath, "Ambinity.zip");
        await Task.Run(() => _client.SftpServer.DownloadFile(selectedRelease.Path, downloadPath, progress));
        //unzip
        if (!File.Exists(downloadPath))
        {
            Log.Error("Unable to download latest release");
            return (null, null);
        }

        return (downloadPath, selectedRelease);
    }

    /// <summary>
    /// extract latest release zip file to updating/installing
    /// </summary>
    /// <returns></returns>
    public async Task ExtractRelease(string file, IProgress<int> progress)
    {
        //clear the installing path first
        if (Directory.Exists(Path.Combine(Constants.UpdatingFolder, "installing")))
            Directory.Delete(Path.Combine(Constants.UpdatingFolder, "installing"), true);
        await using FileStream fileStream = new FileStream(file, FileMode.Open);
        ZipArchive archive = new ZipArchive(fileStream);
        float count = 0;
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            using (Stream unzippedEntryStream = entry.Open())
            {
                progress.Report((int)(count / archive.Entries.Count * 100f));
                if (entry.Length > 0)
                {
                    string path = Path.Combine(Constants.UpdatingFolder, "installing", entry.FullName);
                    CreateDirectoryForFile(path);
                    using (Stream extractStream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        await unzippedEntryStream.CopyToAsync(extractStream);
                    }
                }
            }

            count++;
        }
    }

    private void CreateDirectoryForFile(string path)
    {
        FileUtilities.CreateAccessibleDirectory(Path.GetDirectoryName(path));
    }

    public async Task<AppReleaseInformation> GetLatestRelease()
    {
        var availableRelease = await GetAvailableRelease();
        var latestRelease = availableRelease.OrderByDescending(r => r.ReleaseDate).First();
        return latestRelease;
    }

    public async Task<List<AppReleaseInformation>> GetAvailableRelease()
    {
        var result = await _client.Init();
        if (!result)
        {
            return (null);
        }

        //download latest release
        var itemsFolder = await _client.SftpServer.GetAllFilesAddressInFolder(_releaseRemotePath);
        if (itemsFolder == null)
        {
            Log.Information("Release folder is empty: " + _releaseRemotePath);
            return (null);
        }

        Log.Information("Updating release: " + _releaseRemotePath);
        var availableRelease = new List<AppReleaseInformation>();
        foreach (var url in itemsFolder)
        {
            //load all release
            var appRelease = await _client.SftpServer.GetFiles<AppReleaseInformation>(url + "/info.json");
            if (appRelease != null)
            {
                appRelease.ReleaseDate = _client.SftpServer.GetFileAttributes(url + "/info.json").LastWriteTime;
                appRelease.Path = url + "/Ambinity.zip";
                availableRelease.Add(appRelease);
            }
        }

        if (availableRelease.Count == 0)
        {
            Log.Information("No release found");
            return (null);
        }

        return availableRelease;
    }
}