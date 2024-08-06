namespace AmbinityServer;

public class AmbinityClient
{
    private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Ambinity\\");

    private string ResourceLocalFolderpath => Path.Combine(JsonPath, "Resources");
    private const string ResourceRemoteFolderPath = "/home/adrilight_developeruser/ftp/files/Resources";
    private SftpWrapper _ftpServer;
    public SftpWrapper SftpServer => _ftpServer;

    public AmbinityClient()
    {
        _ftpServer = new SftpWrapper();
    }

    public bool Init()
    {
        //dispose first
        
        try
        {
            _ftpServer.Connect();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Download necessary files for first time app running
    /// </summary>
    public async Task DownloadAssets(IProgress<int> progress)
    {
        if (!Directory.Exists(ResourceLocalFolderpath))
            await _ftpServer.DownloadDirectory(ResourceRemoteFolderPath, ResourceLocalFolderpath, progress);
    }

    public void Disconnect()
    {
        _ftpServer.Disconnect();
    }
}