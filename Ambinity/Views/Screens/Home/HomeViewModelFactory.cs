using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.Home;

public class HomeViewModelFactory
{
    private ThumbnailService _thumbnailService;
    public HomeViewModelFactory(ThumbnailService thumbnailService)
    {
        _thumbnailService = thumbnailService;
    }

    public HyperLinkHomeViewModel GetHyperLinkViewModel(OnlineItem item)
    {
        return new HyperLinkHomeViewModel(item, _thumbnailService);
    }
}