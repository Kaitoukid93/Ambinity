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

public class SideMenuProfilePlayerMiniViewModel : ViewModelBase
{
    public event Action<LightingProfile> PlayingButtonClicked;

    public SideMenuProfilePlayerMiniViewModel(LightingProfileDecoder decoder, RootNavigationStores rootNavigationStores,
         IMainWindowService windowService, GeneralSettingsManager generalSettingsManager)
    {
        _generalSettings = generalSettingsManager.Settings;
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
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
        _decoder.ShouldUpdateFrame = true;
    }

    private void OnMainWindowClosed(object? sender, EventArgs e)
    {
        _decoder.ShouldUpdateFrame = false;
    }

    private WriteableBitmap _reusableBitmap;

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

    private readonly IMainWindowService _windowService;
    private readonly IGeneralSettings _generalSettings;


    public ICommand GoToProfileEditorCommand { get; set; }
    public ICommand PlayButtonCommand { get; set; }
    public string PlayButtonToolTip => IsPlaying ? "Pause" : "Play";
    public string PlayButtonIcon => IsPlaying ? "pauseComposition" : "playComposition";

    public string CurrentPlayingProfileDescription => (CurrentPlayingProfile == null || CurrentPlayingProfile.Category == null)
        ? "Last played profile"
        : CurrentPlayingProfile.Category.Name;

}
