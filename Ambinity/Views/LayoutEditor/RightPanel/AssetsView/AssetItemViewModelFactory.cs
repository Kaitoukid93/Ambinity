using System.Collections.Generic;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.CollectableItem.LightingZone;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Repositories;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// resolve item viewmodel to adapt to each data model ( ambinity device layout, solidcolor...)
/// </summary>
public  class AssetItemViewModelFactory
{
    private readonly Draw2DCanvasViewModel _canvasViewModel;

    public AssetItemViewModelFactory(Draw2DCanvasViewModel canvasViewModel)
    {
        _canvasViewModel = canvasViewModel;
    }
    public  AssetItemViewModelBase GetViewModel(ICollectableItem item)
    {
        switch (item.GetType().Name)
        {
            case "AmbinityDeviceLayout":
                return new AmbinityDeviceLayoutAssetViewModel(item,_canvasViewModel);
                break;
            case "SolidColor":
                return null;
                break;
            case "ColorPalette":
                return new ColorPaletteAssetViewModel(item);
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