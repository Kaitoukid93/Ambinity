using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Root;
using AmbinityCore.DataBase;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Profile;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core.Graphic;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfilePlayerViewModel : ViewModelBase
{
    public event Action<LightingProfile> PlayingButtonClicked;
    public event Action ProfilePictureUpdated;
    private bool _shouldShowImage;

    public SideMenuProfilePlayerViewModel(LightingProfileDecoder decoder, RootNavigationStores rootNavigationStores,
        FrameBuffer frame, IMainWindowService windowService, GeneralSettingsManager generalSettingsManager)
    {
        _generalSettings = generalSettingsManager.Settings;
        _decoder = decoder;
        _frame = frame;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        _decoder.FrameUpdate += OnFrameUpdate;
        CurrentPlayingProfile = _decoder.CurrentPlayingProfile;
        IsPlaying = _decoder.IsRendering;
        _rootNavigationStores = rootNavigationStores;
        GoToProfileEditorCommand = new RelayCommand(GotoProfileEditor);
        PlayButtonCommand = new RelayCommand(ToggleRenderingStatus);
        _windowService = windowService;
        _windowService.MainWindowClosed += OnMainWindowClosed;
        _windowService.MainWindowOpened += OnMainWindowOpened;
    }

    private void ToggleRenderingStatus()
    {
        _decoder.Toggle();
    }

    private void OnMainWindowOpened(object? sender, EventArgs e)
    {
        _shouldShowImage = true;
        _decoder.ShouldUpdateFrame = true;
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        _shouldShowImage = false;
        _decoder.ShouldUpdateFrame = false;
    }

    private WriteableBitmap _reusableBitmap;

    private void OnFrameUpdate()
    {
        if (!_shouldShowImage)
            return;
        if (_reusableBitmap == null)
        {
            int width = _frame.FrameWidth;
            int height = _frame.FrameHeight;
            PixelFormat pixelFormat = PixelFormat.Bgra8888;
            AlphaFormat alphaFormat = AlphaFormat.Premul;
            _reusableBitmap = new WriteableBitmap(
                new PixelSize(width, height),
                new Vector(96, 96),
                pixelFormat,
                alphaFormat);
        }

        using (var frameBuffer = _reusableBitmap.Lock())
        {
            // Copy the byte array data into the pixel buffer
            Marshal.Copy(_frame.PixelData, 0, frameBuffer.Address, _frame.PixelData.Length);
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            ProfileBitmap = _reusableBitmap;
            ProfilePictureUpdated?.Invoke();
        });
    }

    private RootNavigationStores _rootNavigationStores;

    private void OnRenderingStatusChanged()
    {
        IsPlaying = _decoder.IsRendering;
        CurrentPlayingProfile = _decoder.CurrentPlayingProfile;
        OnPropertyChanged(nameof(CurrentPlayingProfileDescription));
        OnPropertyChanged(nameof(PlayButtonIcon));
        OnPropertyChanged(nameof(PlayButtonToolTip));
    }

    private void GotoProfileEditor()
    {
        if (CurrentPlayingProfile == null)
            return;
        PlayingButtonClicked?.Invoke(CurrentPlayingProfile);
    }

    private LightingProfileDecoder _decoder;
    private bool _isPlaying;

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    private LightingProfile _currentPlayingProfile;

    public LightingProfile CurrentPlayingProfile
    {
        get => _currentPlayingProfile;
        set
        {
            _currentPlayingProfile = value;
            OnPropertyChanged();
        }
    }

    private WriteableBitmap? _profileBitmap;
    private readonly FrameBuffer _frame;
    private readonly IMainWindowService _windowService;
    private readonly IGeneralSettings _generalSettings;

    public WriteableBitmap? ProfileBitmap
    {
        get => _profileBitmap;
        set
        {
            _profileBitmap = value;
            OnPropertyChanged();
        }
    }

    public ICommand GoToProfileEditorCommand { get; set; }
    public ICommand PlayButtonCommand { get; set; }
    public string PlayButtonToolTip => IsPlaying ? "Pause" : "Play";
    public string PlayButtonIcon => IsPlaying ? "pauseComposition" : "playComposition";

    public string CurrentPlayingProfileDescription => (CurrentPlayingProfile == null || CurrentPlayingProfile.Category == null)
        ? "Last played profile"
        : CurrentPlayingProfile.Category.Name;

}
