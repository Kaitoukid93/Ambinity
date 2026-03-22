using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using AmbinityCore.Repositories;

namespace Ambinity.Views.SideMenu;

public class ProfilePropertiesEditorViewModel : WindowDialogViewModelBase
{
    public ProfilePropertiesEditorViewModel(ThumbnailService thumbnailService,
     LightingProfileItem profile,
     AssetLifecycleService assetLifecycleService,
        IWindowService windowService,
        IDialogService dialogService)
    {
        _dialogService = dialogService;
        _profile = profile;
        _thumbnailService = thumbnailService;
        Name = _profile.Name;
        Description = _profile.Description;
        _windowService = windowService;
        _icon = _profile.Icon;
        _iconPath = _profile.Thumbnail;
        _lifecycle = assetLifecycleService;
        SelectBitmapCommand = new AsyncRelayCommand(ExecuteBrowseBitmapFile);
        SelectIconCommand = new AsyncRelayCommand(ExecuteBrowseIcon);
    }

    private async Task ExecuteBrowseIcon()
    {
        var vm = new GeometryPickerViewModel(this);
        await _dialogService.ShowWindowDialog(vm, "Icon browser", "Done", "Cancel");
    }

    private LightingProfileItem _profile;
    private readonly ThumbnailService _thumbnailService;
    private IWindowService _windowService;
    private readonly AssetLifecycleService _lifecycle;
    // private IconTypeEnum _iconType;

    // public IconTypeEnum IconType
    // {
    //     get => _iconType;
    //     set
    //     {
    //         _iconType = value;
    //         _profile.IconType = value;
    //         OnPropertyChanged();
    //     }
    // }

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
    private Color _iconColor;

    public Color IconColor
    {
        get => _iconColor;
        set
        {
            _iconColor = value;

            OnPropertyChanged();
        }
    }
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

    protected override async Task OnClosedAsync(
      ContentDialog sender,
      ContentDialogClosedEventArgs args)
    {
        if (args.Result != ContentDialogResult.Primary)
            return;

        var payload = new UpdateProfilePayload
        {
            Name = Name,
            Description = Description,
            ThumbnailPath = _iconPath,
        };

        await _lifecycle.UpdateAsync(_profile.Id, payload);
    }

    public string Name { get; set; }
    public string Description { get; set; }
    private string _iconPath;
    private readonly IDialogService _dialogService;
    public ICommand SelectBitmapCommand { get; }
    public ICommand SelectIconCommand { get; }
}
