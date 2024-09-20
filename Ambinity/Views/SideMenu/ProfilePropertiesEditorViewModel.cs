using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using Ambinity.Windows;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;

namespace Ambinity.Views.SideMenu;

public class ProfilePropertiesEditorViewModel : WindowDialogViewModelBase
{
    public ProfilePropertiesEditorViewModel(ThumbnailService thumbnailService, LightingProfile profile,
        IWindowService windowService, IDialogService dialogService)
    {
        _dialogService = dialogService;
        _profile = profile;
        _thumbnailService = thumbnailService;
        Name = _profile.Name;
        Description = _profile.Description;
        _windowService = windowService;
        _iconType = _profile.IconType;
        _icon = _profile.Icon;
        _iconColor = _profile.IconColor;
        _iconPath = Path.Combine(_profile.LocalPath, "icon.png");
        SelectBitmapCommand = new AsyncRelayCommand(ExecuteBrowseBitmapFile);
        SelectIconCommand = new AsyncRelayCommand(ExecuteBrowseIcon);
    }

    private async Task ExecuteBrowseIcon()
    {
        var vm = new GeometryPickerViewModel(this);
        await _dialogService.ShowWindowDialog(vm, "Icon browser", "Done", "Cancel");
    }

    private LightingProfile _profile;
    private readonly ThumbnailService _thumbnailService;
    private IWindowService _windowService;

    private IconTypeEnum _iconType;

    public IconTypeEnum IconType
    {
        get => _iconType;
        set
        {
            _iconType = value;
            _profile.IconType = value;
            OnPropertyChanged();
        }
    }

    private string _icon;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            _profile.Icon = value;
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
            _profile.IconColor = value;
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
        IconType = IconTypeEnum.Image;
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
            _thumbnailService.ClearCache(dest);
        }

        _profile.UpdateIcon();
        DialogClosed?.Invoke(this, args);
    }

    public string Name { get; set; }
    public string Description { get; set; }
    private string _iconPath;
    private readonly IDialogService _dialogService;
    public ICommand SelectBitmapCommand { get; }
    public ICommand SelectIconCommand { get; }
}