using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneOnlineRepository : OnlineItemRepository
{
    public LightingZoneOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = _client.HomeAddress+ "/ftp/files/LightingZones";
    }
}