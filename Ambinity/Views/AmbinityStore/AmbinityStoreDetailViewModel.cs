using System.Threading.Tasks;
using Ambinity.ViewModels;
using Ambinity.Views.OnlineStore;
using AmbinityServer.OnlineItem;

namespace Ambinity.Views.AmbinityStore;

public class AmbinityStoreDetailViewModel : ViewModelBase
{
    public AmbinityStoreDetailViewModel()
    {
        
    }

    private OnlineItemAssetViewModel _item;
    public OnlineItemAssetViewModel Item
    {
        get => _item;
        set
        {
            _item = value;
            OnPropertyChanged();
        }
    }
    private string _mdText;
    public string MDText
    {
        get => _mdText;
        set
        {
            _mdText = value;
            OnPropertyChanged();
        }
    }

    public string Type
    {
        get
        {
            switch (_item.OnlineItemData.Type)
            {
                case OnlineItemTypeEnum.Animation:
                    return "Animation";
                case OnlineItemTypeEnum.ColorPalette:
                    return "Color Palette";
                case OnlineItemTypeEnum.DeviceLayout:
                    return "Device Lauyout";
                case OnlineItemTypeEnum.LightingProfile:
                    return "Lighting Profile";
                case OnlineItemTypeEnum.LightingZone:
                    return "Lighting Zone";
                case OnlineItemTypeEnum.Gif:
                    return "Gif";
                default:
                    return "Unknown";
            }
        }
    }
    public async Task Init(OnlineItemAssetViewModel item)
    {
        Item = item;
        //get markdown
        MDText = await item.GetMarkdownDescription();
    }
    
}
