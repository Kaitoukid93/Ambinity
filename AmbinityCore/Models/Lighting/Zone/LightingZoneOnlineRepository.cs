using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneOnlineRepository : OnlineItemRepository
{
    public LightingZoneOnlineRepository(AmbinityClient _client) : base(_client)
    {
        
    }
}