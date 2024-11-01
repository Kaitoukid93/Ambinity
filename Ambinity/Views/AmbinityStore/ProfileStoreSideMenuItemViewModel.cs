using Ambinity.ViewModels;

namespace Ambinity.Views.AmbinityStore;

public class ProfileStoreSideMenuItemViewModel : ViewModelBase
{
    public ProfileStoreSideMenuItemViewModel()
    {
        
    }
    public string Icon { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Filter { get; set; }
}