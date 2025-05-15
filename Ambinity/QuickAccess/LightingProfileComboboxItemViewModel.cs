
using System;
using System.IO;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.QuickAccess;

public class LightingProfileComboboxItemViewModel : ViewModelBase
{
    private ThumbnailService _thumbnailService;
    private LightingProfile _profile;
    public LightingProfileComboboxItemViewModel(LightingProfile profile)
    {
        _thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
        _profile = profile;
    }

    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumbnailPath = Path.Combine(_profile.LocalPath, "icon.png");
        if (!File.Exists(thumbnailPath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(thumbnailPath);
        return thumb;
    }
    public string Name => _profile.Name;
    public Guid ID => _profile.ID;
    public string Icon => _profile.Icon;
    public Color IconColor => _profile.IconColor;
    public IconTypeEnum IconType => _profile.IconType;

}
