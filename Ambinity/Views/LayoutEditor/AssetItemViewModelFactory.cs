using System.Collections.Generic;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.CollectableItem.LightingZone;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// resolve item viewmodel to adapt to each data model ( ambinity device layout, solidcolor...)
/// </summary>
public static class AssetItemViewModelFactory
{
    
    public static AssetItemViewModelBase GetViewModel(ICollectableItem item)
    {
        switch (item.GetType().Name)
        {
            case "AmbinityDeviceLayout":
                return new AmbinityDeviceLayoutAssetViewModel(item);
                break;
            case "SolidColor":
                return null;
                break;
            case "ColorPalette":
                return null;
                break;
            case "LightingProfile":
                return null;
                break;
            case "LightingZone":
                return new LightingZoneAssetViewModel(item);
                break;
            case "Animation":
                return null;
                break;
            case "Gif":
                return null;
                break;
            default:
                return null;
        }
    }
}