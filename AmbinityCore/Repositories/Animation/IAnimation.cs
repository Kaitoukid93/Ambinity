using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;

namespace AmbinityCore.Repositories;
/// <summary>
/// Represent a animation data that is hard coded, such as json animation or video
/// well technically json animation can change color at runtime but that's just not so helpful
/// </summary>
public interface IAnimation : ICollectableItem
{
    void LoadAnimation();

    Guid UID { get; set; }

    TimeSpan Duration { get; }

    string Fps { get;}

    string Size { get;}

    string Version {get;}

}
