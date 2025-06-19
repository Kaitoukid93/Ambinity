using Serilog;

namespace AmbinityServer;

public class AmbinityClient
{

    private SftpWrapper _ftpServer;
    public SftpWrapper SftpServer => _ftpServer;

    public string HomeAddress = "/home/adrilight_enduser/";
    public AmbinityClient()
    {
        _ftpServer = new SftpWrapper("adrilight_publicuser","@drilightPublic");
    }

    public async Task<bool> Init()
    {
        return await _ftpServer.Connect();
    }


    public void Disconnect()
    {
        Log.Information("Disposing connection to Ambinity server...");
        _ftpServer.Disconnect();
        Log.Information("Ambinity server connection is disposed");
    }
}
