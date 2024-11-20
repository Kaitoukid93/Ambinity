using AmbinityServer.Download;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Serilog;

namespace AmbinityServer;

public class SftpWrapper
{
    private const string public_User_LoginName = "adrilight_publicuser";
    private const string public_User_PassWord = "@drilightPublic";
    private string developer_User_Login_Name;
    private string developer_User_Password;
    private const string host = @"103.148.57.184";
    private CancellationTokenSource _cancellationTokenSource;
    public SftpClient sFTP { get; set; }

    public SftpWrapper(string userName, string password)
    {
        developer_User_Login_Name = userName;
        developer_User_Password = password;
        sFTP = new SftpClient(host, 1512, developer_User_Login_Name, developer_User_Password);
        _progress = new Progress<int>((p) =>
        {
           Log.Information(p+"%");
        });
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task<bool> Connect()
    {
        if (sFTP.IsConnected)
            return true;
        try
        {
            await sFTP.ConnectAsync(_cancellationTokenSource.Token);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return false;
        }

        
    }

    public void Disconnect()
    {
        try
        {
            sFTP.Disconnect();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }

    public void Dispose()
    {
        sFTP.Dispose();

        GC.SuppressFinalize(this);
    }

    private IProgress<int> _progress;
    private long _itemSize;
    private string _itemName;

    public async Task<List<String>> GetAllFilesAddressInFolder(string folderPath)
    {
        var listFilesAddress = new List<String>();

        try
        {
            var files = sFTP.ListDirectory(folderPath);

            foreach (var file in files.Where(i => i.Name != "." && i.Name != ".."))
            {
                listFilesAddress.Add(folderPath + "/" + file.Name);
            }

            return await Task.FromResult(listFilesAddress);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been caught " + e.ToString());
            return null;
        }
    }

    public async Task<String> GetFileByName(string fileName, string folderPath)
    {
        var listFilesAddress = new List<String>();

        try
        {
            var files = sFTP.ListDirectory(folderPath);

            var file = files.Where(i => i.Name == fileName).FirstOrDefault();

            return await Task.FromResult(folderPath + "/" + file.Name);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been caught " + e.ToString());
            return null;
        }
    }

    public async Task<ISftpFile> GetFileByNameMatching(string fileName, string folderPath)
    {
        var listFilesAddress = new List<String>();

        try
        {
            var files = sFTP.ListDirectory(folderPath);

            var file = files.Where(i => i.Name.Contains(fileName)).FirstOrDefault();

            return await Task.FromResult(file);
        }
        catch (Exception ex)
        {
            Log.Warning(ex.ToString());
            Console.WriteLine("An exception has been caught " + ex.ToString());
            return null;
        }
    }

    public async Task<List<ISftpFile>> GetAllFilesInFolder(string folderPath)
    {
        var listFiles = new List<ISftpFile>();

        try
        {
            var files = sFTP.ListDirectory(folderPath);

            foreach (var file in files.Where(i => i.Name != "." && i.Name != ".."))
            {
                listFiles.Add(file);
            }

            return await Task.FromResult(listFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception has been caught " + ex.ToString());
            Log.Warning(ex.ToString());
            return null;
        }
    }

    public SftpFileAttributes GetFileAttributes(string remotePath)
    {
        SftpFileAttributes attrs = sFTP.GetAttributes(remotePath);
        return attrs;
    }

    public ISftpFile GetFileOrFolderName(string remotePath)
    {
        return sFTP.Get(remotePath);
    }

    private void DownloadProgres(ulong donwloaded)
    {
        // Update progress bar on foreground thread
        Log.Information($"Downloading: " + _itemName);
        _progress.Report((int)((100 * donwloaded) / (ulong)_itemSize));
        
    }

    public bool IsFolder(string path)
    {
        var att = sFTP.GetAttributes(path);
        if (att.IsDirectory)
            return true;
        return false;
    }

    public bool IsExist(string path)
    {
        return sFTP.Exists(path);
    }
    /// <summary>
    /// download with internal progress
    /// </summary>
    /// <param name="remotePath"></param>
    /// <param name="localPath"></param>
    public void DownloadFile(string remotePath, string localPath)
    {
        if (File.Exists(localPath))
            return;

        try
        {
            using (var s = System.IO.File.Create(localPath))
            {
                _itemSize = GetFileAttributes(remotePath).Size;
                _itemName = remotePath;
                sFTP.DownloadFile(remotePath, s, DownloadProgres);
            }
        }
        catch (System.IO.IOException ex)
        {
            Log.Warning(ex.ToString());
        }
        catch (Exception ex)
        {
            Log.Warning(ex.ToString());
        }
    }
    /// <summary>
    /// download with external progress
    /// </summary>
    /// <param name="remotePath"></param>
    /// <param name="localPath"></param>
    public void DownloadFile(string remotePath, string localPath, IProgress<int> progress)
    {
        _progress = progress;
        if (File.Exists(localPath))
            return;

        try
        {
            using (var s = System.IO.File.Create(localPath))
            {
                _itemSize = GetFileAttributes(remotePath).Size;
                _itemName = remotePath;
                sFTP.DownloadFile(remotePath, s, DownloadProgres);
            }
        }
        catch (System.IO.IOException ex)
        {
            Log.Warning(ex.ToString());
        }
        catch (Exception ex)
        {
            Log.Warning(ex.ToString());
        }
    }
    public async Task DownloadDirectory(string sourceRemotePath, string destLocalPath,
        IProgress<DownloadProgress> progress = null)
    {
        Log.Information("Start downloading: " + sourceRemotePath);
        Directory.CreateDirectory(destLocalPath);
        IEnumerable<ISftpFile> files = sFTP.ListDirectory(sourceRemotePath);
        var step = 100 / files.Count();
        DownloadProgress currentProgress = new DownloadProgress();
        foreach (SftpFile file in files)
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                string sourceFilePath = sourceRemotePath + "/" + file.Name;
                string destFilePath = Path.Combine(destLocalPath, file.Name);
                if (file.IsDirectory)
                {
                    await DownloadDirectory(sourceFilePath, destFilePath);
                }
                else
                {
                    DownloadFile(sourceFilePath, destFilePath);
                }

                for (int i = 0; i < step; i++)
                {
                    currentProgress.Progress += 1;
                    currentProgress.Status = "Downloading" + " : " + file.Name;
                    
                    
                    await Task.Delay(1);
                    progress?.Report(currentProgress);
                }
            }
        }
        Log.Information("Download complete!");
    }
    static string GetProgressString(int current, int total)
    {
        const int maxDots = 20; // Maximum number of dots
        int dotsToShow = (int)Math.Round((double)current / total * maxDots);
        return new string('.', dotsToShow);
    }
    public async Task<T> GetFiles<T>(string filePath) //only use for text format
    {
        try
        {
            T file;

            using (var remoteFileStream = sFTP.OpenRead(filePath))
            {
                var textReader = new System.IO.StreamReader(remoteFileStream);
                string s = textReader.ReadToEnd();
                file = JsonConvert.DeserializeObject<T>(s);
            }

            return await Task.FromResult(file);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been caught " + e.ToString());
            return default(T);
        }
    }

    public async Task<string> GetStringContent(string filePath) //only use for text format
    {
        try
        {
            string content;


            using (var remoteFileStream = sFTP.OpenRead(filePath))
            {
                var textReader = new System.IO.StreamReader(remoteFileStream);
                content = textReader.ReadToEnd();
            }

            return await Task.FromResult(content);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been caught " + e.ToString());
            return "";
        }
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
        var serializer = new JsonSerializer();
        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            return serializer.Deserialize<T>(jsonTextReader);
        }
    }

    public async Task<Stream>
        GetThumb(string thumbPath) // this method get all file from dropbox adrilight App folder to temp folder
    {
        try
        {
            return sFTP.OpenRead(thumbPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex.ToString());
            Console.WriteLine("An exception has been caught " + ex.ToString());
            return null;
        }
    }
}