using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.SideMenu;

public class ProfilePropertiesEditorViewModel : WindowDialogViewModelBase
{
    public ProfilePropertiesEditorViewModel(ThumbnailService thumbnailService, LightingProfile profile,
        IWindowService windowService)
    {
        _profile = profile;
        _thumbnailService = thumbnailService;
        Name = _profile.Name;
        Description = _profile.Description;
        _windowService = windowService;
        _iconPath = Path.Combine(_profile.LocalPath, "icon.png");
        SelectBitmapCommand = new AsyncRelayCommand(ExecuteBrowseBitmapFile);
    }

    private LightingProfile _profile;
    private readonly ThumbnailService _thumbnailService;
    private IWindowService _windowService;
    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumbnailPath = Path.Combine(_iconPath);
        if (!File.Exists(thumbnailPath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(thumbnailPath);
        return thumb;
    }

    private async Task ExecuteBrowseBitmapFile()
    {
        string[]? result = await _windowService.CreateOpenFileDialog()
            .HavingFilter(f => f.WithBitmaps())
            .ShowAsync();

        if (result == null)
            return;
        _iconPath = result.First();
        OnPropertyChanged(nameof(GetThumbnail));
    }

    public override void DialogOnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Dialog.Closed -= DialogOnClosed;
        var result = args.Result;
        if (result != ContentDialogResult.Primary)
            return;
        //save data here and notify icon change
        _profile.Name = Name;
        _profile.Description = Description;
        var dest = Path.Combine(_profile.LocalPath, "icon.png");
        if (dest != _iconPath)
        {
            File.Copy(_iconPath, dest, true);
            //clear cache
            _thumbnailService.ClearCache(Path.Combine(dest, "icon.png"));
            _profile.UpdateIcon();
        }

        DialogClosed?.Invoke(this, args);
    }

    public string Name { get; set; }
    public string Description { get; set; }
    private string _iconPath;
    public ICommand SelectBitmapCommand { get; set; }
}