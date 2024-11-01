using AmbinityCore.Repositories;
using AmbinityServer;

namespace Ambinity.Views.Screens.Home;

public class TutorialsOnlineRepository : OnlineItemRepository
{
    public TutorialsOnlineRepository(AmbinityClient client) : base(client)
    {
        ResourceAddress = client.HomeAddress+ "/ftp/files/Tutorials";
    }
}