using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.CollectableItem.LightingZone;

public class LightingZoneAssetViewModel : AssetItemViewModelBase
{
    public LightingZoneAssetViewModel(ICollectableItem item) : base(item)
    {
        _item = item as AmbinityCore.Models.Lighting.Zone.LightingZone;
        Name = _item.Name;
        Icon = _item.LightingConfiguration.Icon;
        Description ="W: " + _item.Width + " - " + "H: " + _item.Height;
        ConfigInfo = _item.LightingConfiguration.GetInfo();
    }
    
    private string _icon;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    private AmbinityCore.Models.Lighting.Zone.LightingZone _item;
    
    private string _description;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }
    public string ConfigInfo { get; set; }
    public DateTime LastUpdate { get; set; }
}