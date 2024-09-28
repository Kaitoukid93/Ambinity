using Serilog;

namespace AmbinityServer;

public class AmbinityClient
{
   
    private SftpWrapper _ftpServer;
    public SftpWrapper SftpServer => _ftpServer;

    public string HomeAddress = "/home/adrilight_developeruser/";

    public AmbinityClient()
    {
        _ftpServer = new SftpWrapper("adrilight_developeruser","@drilightDeveloper");
    }

    public async Task<bool> Init()
    {
        //dispose first
        return await _ftpServer.Connect();
    }
    

    public void Disconnect()
    {
        Log.Information("Disposing connection to Ambinity server...");
        _ftpServer.Disconnect();
        Log.Information("Ambinity server connection is disposed");
    }
}