using System.Collections.Generic;
using System.Linq;
using Ambinity.ViewModels;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ProfileEditorViewModel : ViewModelBase
{
    public ProfileEditorViewModel( ZoneMappingViewModel zoneMappingViewModel, LayersViewModel layersViewModel)
    {
        ZoneMappingViewModel = zoneMappingViewModel;
        LayersViewModel = layersViewModel;

    }

    /// <summary>
    /// initialize view with new profile
    /// </summary>
    /// <param name="profile"></param>
    public ZoneMappingViewModel ZoneMappingViewModel { get; set; }
    public LayersViewModel LayersViewModel { get; set; }
    public void Init(LightingProfile profile)
    {
        //Load Zone Mapping View
        ZoneMappingViewModel.Init(profile.Zones.ToList());
        //Load layers view
        LayersViewModel.Update();
        
    }
 
}