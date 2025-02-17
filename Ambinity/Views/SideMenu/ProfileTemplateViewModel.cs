using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;

namespace Ambinity.Views.SideMenu;

public class ProfileTemplateViewModel : ViewModelBase
{
    public ProfileTemplateViewModel(LightingProfile profile,string name, string description)
    {
        Profile = profile;
        Name = name;
        Description = description;
    }

    public LightingProfile Profile { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSelected { get; set; }
}