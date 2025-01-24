using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.OnlineStore;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.AmbinityStore;

public class AmbinityStoreDetailViewModel : ViewModelBase
{
    public AmbinityStoreDetailViewModel()
    {
        PrevImageCommand = new RelayCommand(PrevImage,CanPrev);
        NextImageCommand = new RelayCommand(NextImage,CanNext);
    }

    private bool CanNext()
    {
        return AvailableScreenshots != null && AvailableScreenshots.Count != 0 &&
               _screenShotIndex < AvailableScreenshots.Count - 1;
    }

    private bool CanPrev()
    {
        return AvailableScreenshots != null && AvailableScreenshots.Count != 0 &&
               _screenShotIndex > 0;
    }

    private int _screenShotIndex;

    private void NextImage()
    {
        if (_screenShotIndex < _availableScreenshot.Count - 1)
        {
            _screenShotIndex++;
        }

        CurrentScreenshot = AvailableScreenshots[_screenShotIndex];
    }

    private void PrevImage()
    {
        if (_screenShotIndex > 0)
        {
            _screenShotIndex--;
            CurrentScreenshot = AvailableScreenshots[_screenShotIndex];
        }
    }

    private List<ScreenshotViewModel> _availableScreenshot;

    public List<ScreenshotViewModel> AvailableScreenshots
    {
        get => _availableScreenshot;
        set
        {
            _availableScreenshot = value;
            OnPropertyChanged();
        }
    }

    private ScreenshotViewModel _currentScreenShot;

    public ScreenshotViewModel CurrentScreenshot
    {
        get => _currentScreenShot;
        set
        {
            _currentScreenShot?.UnSelect();
            _currentScreenShot = value;
            _currentScreenShot.Select();
            OnPropertyChanged();
            NextImageCommand.NotifyCanExecuteChanged();
            PrevImageCommand.NotifyCanExecuteChanged();
        }
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

    public RelayCommand PrevImageCommand { get; }
    public RelayCommand NextImageCommand { get; }
    
    //todo cleanup init logic

    public async Task Init(OnlineItemAssetViewModel item)
    {
        Item = item;
        //get markdown
        MDText = await item.GetMarkdownDescription();
        AvailableScreenshots = await item.GetScreenshotAsync();
        if (AvailableScreenshots != null && AvailableScreenshots.Count() > 0)
        {
            _screenShotIndex = 0;
            CurrentScreenshot = AvailableScreenshots.First();
        }
           
    }
}