using System.IO;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor;
using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class GifAnimationThumbnailViewModel : ViewModelBase
{
    public GifAnimationThumbnailViewModel(string thumbnailPath)
    {
        var thumbnailService = Ioc.Default.GetRequiredService<ThumbnailService>();
        ThumbnailStream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read);
    }

    private Stream _thumbnailStream;

    public Stream ThumbnailStream
    {
        get => _thumbnailStream;

        set
        {
            _thumbnailStream = value;
            OnPropertyChanged();
        }
    }
}
