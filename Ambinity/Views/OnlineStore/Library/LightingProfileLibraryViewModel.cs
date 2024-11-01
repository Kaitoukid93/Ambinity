using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using AmbinityCore.Models.Profile;

namespace Ambinity.Views.OnlineStore.Library;

public class LightingProfileLibraryViewModel : LibraryViewModelBase
{
    public LightingProfileLibraryViewModel(LightingProfileRepository lightingProfileRepository,
        LightingProfileOnlineRepository onlineRepository,
        LightingProfileAssetsViewModel profileAssetsViewModel) : base(lightingProfileRepository,
        onlineRepository, profileAssetsViewModel)
    {
    }
}