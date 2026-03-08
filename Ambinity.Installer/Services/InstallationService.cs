using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Ambinity.Installer.Utilities;
using AmbinityServer;
using AmbinityServer.AppRelease;
using Microsoft.Win32;
using Serilog;

namespace Ambinity.Installer.Services;

public class InstallationService
{
    /// <summary>
    /// Code from Artermis.Installer
    /// </summary>
    private readonly string _ambinityStartMenuDirectory;

    private string _releaseRemotePath;
    private readonly AmbinityClient _client;

    public InstallationService(AmbinityClient client)
    {
        _client = client;
        _releaseRemotePath = _client.HomeAddress + "ftp/files/AppRelease";
        RegistryKey installKey = GetInstallKey();
        InstallationDirectory = installKey != null
            ? installKey.GetValue("InstallLocation").ToString()
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ambinity");
        DataDirectory = Constants.AppDataFolder;

        _ambinityStartMenuDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Windows\Start Menu\Programs\Ambinity"
        );
    }

    private void CreateDirectoryForFile(string path)
    {
        FileUtilities.CreateAccessibleDirectory(Path.GetDirectoryName(path));
    }

    private async Task DeleteAppData(IProgress<int> progress)
    {
        // Get all the files recursively as our total
        string directory = Constants.AppDataFolder;
        if (!Directory.Exists(directory))
        {
            progress.Report(100);
            return;
        }

        string source = Assembly.GetEntryAssembly()?.Location;
        string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

        // Delete all files
        await Task.Run(() =>
        {
            int index = 0;
            foreach (string file in files)
            {
                if (file != source)
                    File.Delete(file);

                index++;
                progress.Report((int)(index / (float)files.Length * 100));
            }
        });

        progress.Report(100);
    }

    private async Task DeleteDevices(IProgress<int> progress)
    {
        // Get all the files recursively as our total
        string directory = Constants.HardwareFolderPath;
        if (!Directory.Exists(directory))
        {
            progress.Report(100);
            return;
        }
        try
        {
            Directory.Delete(directory, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        progress.Report(100);
    }
    

    private async Task DeleteData(IProgress<int> progress)
    {
        // Get all the files recursively as our total
        string directory = Constants.ModelDataFolder;
        if (!Directory.Exists(directory))
        {
            progress.Report(100);
            return;
        }

        try
        {
            Directory.Delete(directory, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }


        progress.Report(100);
    }

    private async Task DeleteAppConfig(IProgress<int> progress)
    {
        // Get all the files recursively as our total
        string configFile = Constants.GeneralSettingsFilePath;
        if (!File.Exists(configFile))
        {
            progress.Report(100);
            return;
        }

        File.Delete(configFile);
        progress.Report(100);
    }

    private void KillThemAll()
    {
        Process[] processes = Process.GetProcessesByName("Ambinity.Windows");
        foreach (Process process in processes)
        {
            try
            {
                process.Kill();
            }
            catch (Exception)
            {
                // ignored I guess
            }
        }
    }

    /// <summary>
    /// Download latest release from GitHub
    /// </summary>
    /// <param name="progress"></param>
    public async Task<(string, AppReleaseInformation)> DownloadRelease(IProgress<int> progress,
        AppReleaseInformation info = null)
    {
        var availableRelease = await GetAvailableRelease();
        if (availableRelease == null || availableRelease.Count == 0)
            return (null, null);

        var selectedRelease = info == null ? availableRelease.OrderByDescending(r => r.ReleaseDate).First() : info;

        //download zip
        if (!Directory.Exists(Constants.CacheFolderPath))
            Directory.CreateDirectory(Constants.CacheFolderPath);

        var downloadPath = Path.Combine(Constants.CacheFolderPath, "Ambinity.zip");

        // Download from GitHub
        bool downloadSuccess = await _client.GitHubClient.DownloadAsset(selectedRelease.Path, downloadPath, progress);

        if (!downloadSuccess || !File.Exists(downloadPath))
        {
            Log.Error("Unable to download latest release from GitHub");
            return (null, null);
        }

        return (downloadPath, selectedRelease);
    }

    public async Task<List<AppReleaseInformation>> GetAvailableRelease()
    {
        // Try GitHub first if client is available
        if (_client.GitHubClient != null)
        {
            try
            {
                Log.Information("Fetching releases from GitHub");
                var availableGithubRelease = await _client.GitHubClient.GetAvailableReleases();

                if (availableGithubRelease != null && availableGithubRelease.Count > 0)
                {
                    Log.Information($"Found {availableGithubRelease.Count} releases on GitHub");
                    return availableGithubRelease;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to fetch from GitHub, falling back to SFTP");
            }
        }

        // Fallback to SFTP if GitHub is not available or fails
        Log.Information("Using SFTP fallback for releases");
        var result = await _client.Init();
        if (!result)
        {
            Log.Error("Failed to connect to SFTP server");
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

    public async Task InstallRelease(string file, IProgress<int> progress)
    {
        CleanUpOnShutdown = false;

        FileUtilities.CreateAccessibleDirectory(DataDirectory);
        FileUtilities.CreateAccessibleDirectory(InstallationDirectory);

        await using (FileStream fileStream = new FileStream(file, FileMode.Open))
        {
            ZipArchive archive = new ZipArchive(fileStream);
            float count = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                using (Stream unzippedEntryStream = entry.Open())
                {
                    progress.Report((int)(count / archive.Entries.Count * 100f));
                    if (entry.Length > 0)
                    {
                        string path = Path.Combine(InstallationDirectory, entry.FullName);
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

        progress.Report(100);

        // Copy installer
        string source = Environment.ProcessPath;
        string target = Path.Combine(DataDirectory, "installer", "Ambinity.Installer.exe");
        if (source != target)
        {
            CreateDirectoryForFile(target);
            File.Copy(source, target, true);
        }

        // Populate the start menu
        if (!Directory.Exists(_ambinityStartMenuDirectory))
            Directory.CreateDirectory(_ambinityStartMenuDirectory);

        ShortcutUtilities.Create(
            Path.Combine(_ambinityStartMenuDirectory, "Ambinity.lnk"),
            Path.Combine(InstallationDirectory, "Ambinity.Windows.exe"),
            "",
            InstallationDirectory,
            "Ambinity",
            "",
            ""
        );
        ShortcutUtilities.Create(
            Path.Combine(_ambinityStartMenuDirectory, "Uninstall Ambinity.lnk"),
            Path.Combine(DataDirectory, "installer", "Ambinity.Installer.exe"),
            "-uninstall",
            InstallationDirectory,
            "Uninstall Ambinity",
            "",
            ""
        );
    }

    public async Task RemoteShutdown()
    {
        // try
        // {
        //     // It is unlikely Artemis is already running in this case, lets just check though
        //     if (!File.Exists(Path.Combine(DataDirectory, "webserver.txt")))
        //         return;
        //
        //     string url = File.ReadAllText(Path.Combine(DataDirectory, "webserver.txt"));
        //     using (HttpClient client = new HttpClient())
        //     {
        //         await client.PostAsync(url + "remote/shutdown", null);
        //         await Task.Delay(2000);
        //     }
        // }
        // catch
        // {
        //     // ignored
        // }
        // finally
        // {
        KillThemAll();
        await Task.Delay(1000);
    }

    public async Task UninstallRelease(IProgress<int> progress, bool onlyDelete)
    {
        string source = Assembly.GetEntryAssembly().Location;

        if (Directory.Exists(InstallationDirectory))
        {
            // Get all the files recursively as our total
            string[] files = Directory.GetFiles(InstallationDirectory, "*", SearchOption.AllDirectories);
            // Delete all files except the installer
            await Task.Run(() =>
            {
                int index = 0;
                foreach (string file in files)
                {
                    if (file != source)
                        File.Delete(file);

                    index++;
                    progress.Report((int)(index / (float)files.Length * 100));
                }
            });
        }

        progress.Report(100);

        if (onlyDelete)
            return;

        // Delete the installer itself after it closes
        CleanUpOnShutdown = true;

        // If needed, repeat for app data
        if (RemoveDevices)
            await DeleteDevices(progress);
        if (RemoveData)
            await DeleteData(progress);
        if (RemoveAppConfig)
            await DeleteAppConfig(progress);
        // if (RemoveAppData)
        //     await DeleteAppData(progress);


        // Clean up the start menu
        if (Directory.Exists(_ambinityStartMenuDirectory))
            Directory.Delete(_ambinityStartMenuDirectory, true);
    }

    public RegistryKey GetInstallKey()
    {
        return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Ambinity", true);
    }

    public void CreateInstallKey()
    {
        RegistryKey key =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Ambinity", true) ??
            Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Ambinity", true);

        key.SetValue("DisplayIcon", Path.Combine(InstallationDirectory, "Ambinity.Windows.exe"),
            RegistryValueKind.String);
        key.SetValue("DisplayName", "Ambinity", RegistryValueKind.String);
        key.SetValue("HelpLink", "https://www.ambino.vn/support", RegistryValueKind.String);
        key.SetValue("InstallLocation", InstallationDirectory, RegistryValueKind.String);
        key.SetValue("Publisher", "Ambino", RegistryValueKind.String);
        key.SetValue("UninstallString",
            $"\"{Path.Combine(DataDirectory, "installer", "Ambinity.Installer.exe")}\" -uninstall",
            RegistryValueKind.String);
        key.SetValue("ModifyPath", $"\"{Path.Combine(DataDirectory, "installer", "Ambinity.Installer.exe")}\"",
            RegistryValueKind.String);
        key.SetValue("URLInfoAbout", "https://www.ambino.vn", RegistryValueKind.String);

        key.Close();
    }

    /// <inheritdoc />
    public void RemoveInstallKey()
    {
        Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Ambinity", false);
    }

    /// <inheritdoc />
    public void CreateDesktopShortcut()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Ambinity.lnk");
        ShortcutUtilities.Create(path, Path.Combine(InstallationDirectory, "Ambinity.Windows.exe"), "",
            InstallationDirectory, "Ambinity", "", "");
    }

    /// <inheritdoc />
    public void RemoveDesktopShortcut()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Ambinity.lnk");
        if (File.Exists(path))
            File.Delete(path);
    }

    public List<string> Args { get; set; }
    public string InstallationDirectory { get; set; }
    public string DataDirectory { get; }
    public bool RemoveAppData { get; set; }
    public bool RemoveData { get; set; }
    public bool RemoveDevices { get; set; }
    public bool RemoveAppConfig { get; set; }
    public bool CleanUpOnShutdown { get; set; }
}
