namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// provide viewmodel for requested library
/// </summary>
public class LibraryViewModelFactory(
    ColorPalettesLibraryViewModel colorPalettesLibraryViewModel,
    AnimationLibraryViewModel animationLibraryViewModel)
{
    public LibraryViewModelBase GetLibraryViewModel(string type)
    {
        switch (type)
        {
            case "ColorPalette":
                return colorPalettesLibraryViewModel;
            case"Animation":
                return animationLibraryViewModel;
            default: return null;
        }
    }
}