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
        return type switch
        {
            "ColorPalette" => colorPalettesLibraryViewModel,
            "Animation" => animationLibraryViewModel,
            _ => null
        };
    }
}