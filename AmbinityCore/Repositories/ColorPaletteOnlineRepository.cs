using AmbinityCore.Models.Collection;
using AmbinityServer;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Repositories;

public class ColorPaletteOnlineRepository : OnlineItemRepository
{
    public ColorPaletteOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = "/home/adrilight_developeruser/ftp/files/ColorPalettes";
    }
}