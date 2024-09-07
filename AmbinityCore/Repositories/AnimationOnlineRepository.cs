using AmbinityServer;

namespace AmbinityCore.Repositories;

public class AnimationOnlineRepository : OnlineItemRepository
{
    public AnimationOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = "/home/adrilight_developeruser/ftp/files/Animations";
    }
}
