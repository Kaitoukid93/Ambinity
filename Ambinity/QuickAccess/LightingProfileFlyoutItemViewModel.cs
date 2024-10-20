using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class LightingProfileFlyoutItemViewModel : ViewModelBase
{
    private LightingProfile _profile;
    private ThumbnailService _thumbnailService;
    private readonly LightingProfileDecoder _decoder;

    public LightingProfileFlyoutItemViewModel(LightingProfile profile,ThumbnailService thumbnailService,LightingProfileDecoder decoder)
    {
        _decoder = decoder;
        _profile = profile;
        _thumbnailService = thumbnailService;
        SelectProfileCommand = new RelayCommand(SelectProfile);
    }
    

    private void SelectProfile()
    {
       // _decoder.Stop();
       // _decoder.Init(this._profile);
    }

    public string Name => _profile.Name;
    public bool IsPlaying => _profile.IsPlaying;
    public string Icon => _profile.Icon;
    public Color IconColor => _profile.IconColor;
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();
    public IconTypeEnum IconType => _profile.IconType;
    public ICommand SelectProfileCommand { get; }

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumbnailPath = Path.Combine(_profile.LocalPath, "icon.png");
        if (!File.Exists(thumbnailPath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(thumbnailPath);
        return thumb;
    }
}