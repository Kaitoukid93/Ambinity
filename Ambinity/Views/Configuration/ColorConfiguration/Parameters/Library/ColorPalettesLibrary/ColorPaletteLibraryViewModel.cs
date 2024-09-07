using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Colors;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// Viewmodel for Colors Library View
/// </summary>
public class ColorPalettesLibraryViewModel : LibraryViewModelBase
{
    public ColorPalettesLibraryViewModel(ColorPaletteRepository paletteRepository,
        ColorPaletteOnlineRepository colorPaletteOnlineRepository,
        ColorPaletteAssetsViewModel colorPaletteAssetsViewModel) : base(
        paletteRepository, colorPaletteOnlineRepository, colorPaletteAssetsViewModel)
    {
    }
}