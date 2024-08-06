using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.CollectableItem.LightingZone;
using AmbinityCore.Models.Collection;
using AmbinityCore.Repositories;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

/// <summary>
/// provide viewmodel for adapt to item ( color palette, solidcolor..)
/// </summary>
public static class ValueEditorViewModelFactory
{
    
    public static ValueEditorViewModelBase GetViewModel(ICollectableItem item)
    {
        switch (item.GetType().Name)
        {
            case "ColorPalette":
                return new PaletteEditorViewModel(item as ColorPalette);
                break;
            case "SolidColor":
                return new StaticColorEditorViewModel(item);
                break;
            case "GradientColor":
                return new StaticColorEditorViewModel(item);
                break;
            default:
                return null;
        }
    }
}