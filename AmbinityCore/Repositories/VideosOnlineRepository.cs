using System;
using AmbinityServer;

namespace AmbinityCore.Repositories;

public class VideosOnlineRepository : OnlineItemRepository
{
 public VideosOnlineRepository(AmbinityClient _client) : base(_client)
    {
        ResourceAddress = _client.HomeAddress+ "/ftp/files/Videos";
    }
}
