using Ambinity.Views.Screens.DeviceLayout.Library;
using Ambinity.Views.Screens.ProfileEditor.Library;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// provide viewmodel for requested library
/// </summary>
public class LibraryViewModelFactory(
    ColorPalettesLibraryViewModel colorPalettesLibraryViewModel,
    AnimationLibraryViewModel animationLibraryViewModel,
    DeviceLayoutsLibraryViewModel deviceLayoutsLibraryViewModel,
    LightingZonesLibraryViewModel lightingZonesLibraryViewModel)
{
    public LibraryViewModelBase GetLibraryViewModel(string type)
    {
        return type switch
        {
            "ColorPalette" => colorPalettesLibraryViewModel,
            "Animation" => animationLibraryViewModel,
            "DeviceLayout" => deviceLayoutsLibraryViewModel,
            "LightingZone" => lightingZonesLibraryViewModel,
            _ => null
        };
    }
}