using AmbinityServer;

namespace AmbinityCore.Repositories;

public class AnimationOnlineRepository : OnlineItemRepository
{
    public AnimationOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = _client.HomeAddress+ "/ftp/files/Animations";
    }
}
