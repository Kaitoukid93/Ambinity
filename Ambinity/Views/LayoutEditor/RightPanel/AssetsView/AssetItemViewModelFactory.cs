using System.Collections.Generic;
using Ambinity.Views.CollectableItem.AmbinityDeviceLayout;
using Ambinity.Views.CollectableItem.LightingZone;
using Ambinity.Views.Configuration.ColorConfiguration.Parameters;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.LayoutEditor;

/// <summary>
/// resolve item viewmodel to adapt to each data model ( ambinity device layout, solidcolor...)
/// </summary>
public  class AssetItemViewModelFactory
{
    private readonly Draw2DCanvasViewModel _canvasViewModel;
    private readonly ThumbnailService _thumnailService;

    public AssetItemViewModelFactory(Draw2DCanvasViewModel canvasViewModel, ThumbnailService thumbnailService)
    {
        _canvasViewModel = canvasViewModel;
        _thumnailService = thumbnailService;
    }
    public  AssetItemViewModelBase GetViewModel(ICollectableItem item)
    {
        switch (item.GetType())
        {
            case OnlineItemTypeEnum.DeviceLayout: 
                return new AmbinityDeviceLayoutAssetViewModel(item,_thumnailService);
                break;
            case OnlineItemTypeEnum.ColorPalette:
                return new ColorPaletteAssetViewModel(item);
                break;
            case OnlineItemTypeEnum.LightingZone:
                return new LightingZoneAssetViewModel(item);
                break;
            case OnlineItemTypeEnum.Animation:
                return new AnimationAssetViewModel(item);
                break;
            case OnlineItemTypeEnum.LightingProfile:
                return null;
                break;
            default:
                return null;
        }
    }
}