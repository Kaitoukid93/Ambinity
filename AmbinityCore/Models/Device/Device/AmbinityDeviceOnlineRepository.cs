using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Device.Device;

public class AmbinityDeviceOnlineRepository : OnlineItemRepository
{
    public AmbinityDeviceOnlineRepository(AmbinityClient _client) : base(_client)
    {
        //hard coded resource address for each repo
        ResourceAddress = "/home/adrilight_developeruser/ftp/files/SupportedDevices";
    }
}