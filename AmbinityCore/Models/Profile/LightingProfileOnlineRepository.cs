using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Models.Profile;

public class LightingProfileOnlineRepository : OnlineItemRepository
{
    public LightingProfileOnlineRepository(AmbinityClient _client) : base(_client)
    {
    }
    
}