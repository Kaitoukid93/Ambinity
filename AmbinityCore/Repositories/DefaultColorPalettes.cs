using Avalonia.Media;

namespace AmbinityCore.Repositories;

public static class DefaultColorPalettes
{
    public static ColorPalette RetroPalette()
    {
        return new ColorPalette()
        {
            Name = "Retro",
            Colors = retro,

        };
    }
    private static Color[] retro = {
        Color.FromRgb(26, 19, 52),
        Color.FromRgb(38,41,74),
        Color.FromRgb(1,84,90),
        Color.FromRgb(1,115,81),
        Color.FromRgb(170,217,98),
        Color.FromRgb(251,191,69),
        Color.FromRgb(239,106,50),
        Color.FromRgb(237,3,69),
        Color.FromRgb(161,42,94),
        Color.FromRgb(113,1,98),
        Color.FromRgb(2,44,125)};
}