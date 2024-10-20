using Ambinity.Views.OnlineStore;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.Screens.Home;

public class ProfileHomeViewModel
{
    private OnlineItemAssetViewModel OnlineItemViewModel { get; set; }

    public ProfileHomeViewModel(OnlineItemAssetViewModel item)
    {
        OnlineItemViewModel = item;
    }
    public string Name { get; set; } 
    
}