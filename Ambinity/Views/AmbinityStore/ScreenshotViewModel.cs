using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Ambinity.ViewModels;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using Renci.SshNet.Sftp;

namespace Ambinity.Views.AmbinityStore;

public class ScreenshotViewModel : ViewModelBase
{
    private ThumbnailService _thumbnailService;
    public ScreenshotViewModel(ThumbnailService _thumbnailService, string path)
    {
        this._thumbnailService = _thumbnailService;
        _path = path;
        if(_path.EndsWith(".gif"))
        IsAnimated = true;
    }

    private string _path;
    private Bitmap _bitmap;

    public Bitmap Image
    {
        get => _bitmap;
        set
        {
            _bitmap = value;
            OnPropertyChanged();
        }
    }

    private Stream _animatedStream;

    public Stream AnimatedStream
    {
        get => _animatedStream;
        set
        {
            _animatedStream = value;
            OnPropertyChanged();
        }
    }
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }

    }

    public async Task Select()
    {
        IsSelected = true;
        //get image
        if(!IsAnimated)
        Image = await _thumbnailService.GetThumbnail(_path,500);
        else
        {
            //download gif to cache
            AnimatedStream = new MemoryStream();
            await _thumbnailService.LoadAnimatedStream(_path, AnimatedStream);
        }
    }
    public async Task UnSelect()
    {
        IsSelected = false;
        //get image
        Image = null;
    }

    private bool _isAnimated;
    public bool IsAnimated
    {
        get => _isAnimated;
        set
        {
            _isAnimated = value;
            OnPropertyChanged();
        }
    }
}