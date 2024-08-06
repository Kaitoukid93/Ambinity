using System.Collections.Generic;
using AmbinityCore.Repositories;
using Avalonia.Media;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// UI logic for palette picker button
/// </summary>
public class PaletteSelectionParameterViewModel : ValueSelectionViewModelBase
{
    public PaletteSelectionParameterViewModel(ColorPalette palette) : base(palette)
    {
        _palette = palette;
        Colors = new List<Color>();
        foreach (var color in _palette.Colors)
        {
            Colors.Add(color);
        }
    }

    private ColorPalette _palette;
    public List<Color> Colors { get; set; }
    private int _colorMode;

}