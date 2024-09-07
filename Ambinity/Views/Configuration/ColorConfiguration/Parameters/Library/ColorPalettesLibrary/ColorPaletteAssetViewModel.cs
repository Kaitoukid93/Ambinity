using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;
using CommunityToolkit.Mvvm.Input;
using Vortice.Mathematics;
using Color = Avalonia.Media.Color;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class ColorPaletteAssetViewModel : AssetItemViewModelBase
{
    
    public ColorPaletteAssetViewModel(ICollectableItem item) : base(item)
    {
        _item = item as ColorPalette;
        Description = "Colors: " + _item.Colors.Length;
        Colors = _item.Colors.ToList();
    }

    private ColorPalette _item;
    private string _description;
    public bool IsLocalExisted { get; set; }
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }
    public List<Color> Colors { get; set; }
}