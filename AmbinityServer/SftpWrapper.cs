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
    private const string developer_User_Login_Name = "adrilight_developeruser";
    private const string developer_User_Password = "@drilightDeveloper";
    private const string host = @"103.148.57.184";
    public SftpClient sFTP { get; set; }

    public SftpWrapper()
    {
        sFTP = new SftpClient(host, 1512, developer_User_Login_Name, developer_User_Password);
    }

    public bool Connect()
    {
        try
        {
            sFTP.Connect();
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            
        }
        return false;
    }

    public void Disconnect()
    {
        sFTP.Disconnect();
   
    }
    public void Dispose()
    {
        sFTP.Dispose();
  
        GC.SuppressFinalize(this);
    }

    private IProgress<int> _progress;
    private long _itemSize;

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

            var file = files.Where(i => i.Name == fileName).FirstOrDefault();

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

    public ISftpFile GetFileOrFoldername(string remotePath)
    {
        return sFTP.Get(remotePath);
    }

    private void DownloadProgresBar(ulong uploaded)
    {
        // Update progress bar on foreground thread
        _progress.Report((int)((100 * uploaded) / (ulong)_itemSize));
    }

    public void DownloadFile(string remotePath, string localPath, IProgress<int> progress = null)
    {
        _progress = progress;
        if (File.Exists(localPath))
            return;

        try
        {
            using (var s = System.IO.File.Create(localPath))
            {
                _itemSize = GetFileAttributes(remotePath).Size;
                sFTP.DownloadFile(remotePath, s, DownloadProgresBar);
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

    public async Task DownloadDirectory(string sourceRemotePath, string destLocalPath, IProgress<int> progress = null)
    {
        Directory.CreateDirectory(destLocalPath);
        IEnumerable<ISftpFile> files = sFTP.ListDirectory(sourceRemotePath);
        var step = 100 / files.Count();
        int currentProgress = 0;
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
                    using (Stream fileStream = File.Create(destFilePath))
                    {
                        sFTP.DownloadFile(sourceFilePath, fileStream);
                    }
                }
            }

            for (int i = 0; i < step; i++)
            {
                currentProgress += 1;
                await Task.Delay(5);
                progress?.Report(currentProgress);
            }
          
        }
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
    public async Task<Stream> GetThumb(string thumbPath)  // this method get all file from dropbox adrilight App folder to temp folder
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