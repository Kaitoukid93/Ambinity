using AmbinityCore.Models.Collection;
using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Repositories;

public class ColorPaletteOnlineRepository : OnlineItemRepository
{
    public ColorPaletteOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = _client.HomeAddress+ "/ftp/files/ColorPalettes";
    }
}