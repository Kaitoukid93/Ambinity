using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ambinity.Services;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityServer.OnlineItem;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Ambinity.Localization;

namespace Ambinity.QuickAccess;

public class LightingProfilePlayerWidgetViewModel : ViewModelBase
{
    private readonly LightingProfileDecoder _decoder;
    private readonly ThumbnailService _thumbnailService;
    private readonly LightingProfileRepository _profileRepository;
    private readonly QuickAccessViewModelFactory _factory;
    private readonly IMainWindowService _mainWindowService;

    public LightingProfilePlayerWidgetViewModel(LightingProfileDecoder decoder, ThumbnailService thumbnailService,
        LightingProfileRepository profileRepository, QuickAccessViewModelFactory factory, IMainWindowService mainWindowService)
    {
        _mainWindowService = mainWindowService;
        _factory = factory;
        _profileRepository = profileRepository;
        _decoder = decoder;
        _thumbnailService = thumbnailService;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        TogglePlayPauseCommand = new RelayCommand(TogglePlayPause);
        GoToProfileEditorCommand = new RelayCommand(GoToProfileEditor);
        OnPropertyChanged(nameof(IsRendering));
        OnPropertyChanged(nameof(CurrentPlayingProfile));
        OnPropertyChanged(nameof(GetThumbnail));
        AvailableLightingProfiles = new List<LightingProfileFlyoutItemViewModel>();
    }

    private void GoToProfileEditor()
    {
        _mainWindowService.OpenMainWindow(CurrentPlayingProfile);
    }

    public void Init()
    {
        AvailableLightingProfiles.Clear();
        foreach (var item in _profileRepository.Items)
        {
            var vm = _factory.GetFlyoutLightingProfileViewModel(item as LightingProfile);
            AvailableLightingProfiles.Add(vm);
        }
    }

    public List<LightingProfileFlyoutItemViewModel> AvailableLightingProfiles { get; set; }

    private void TogglePlayPause()
    {
        _decoder.Toggle();
    }

    public Task<Bitmap> GetThumbnail => GetThumbnailAsync();

    private async Task<Bitmap> GetThumbnailAsync()
    {
        var thumbnailPath = Path.Combine(CurrentPlayingProfile.LocalPath, "icon.png");
        if (!File.Exists(thumbnailPath))
            return null;
        var thumb = await _thumbnailService.LoadThumbnail(thumbnailPath);
        return thumb;
    }

    private void OnRenderingStatusChanged()
    {
        OnPropertyChanged(nameof(IsRendering));
        OnPropertyChanged(nameof(CurrentPlayingProfile));
        OnPropertyChanged(nameof(GetThumbnail));
        OnPropertyChanged(nameof(ShowEditButton));
        OnPropertyChanged(nameof(Content));
    }

    public bool IsRendering => _decoder.IsRendering;
    public bool ShowEditButton => _decoder.CurrentPlayingProfile != null;
    public LightingProfile CurrentPlayingProfile => _decoder.CurrentPlayingProfile;
    public string Content => CurrentPlayingProfile != null ? Loc.TryGetTranslated(CurrentPlayingProfile.Name + ".Profile.Name", CurrentPlayingProfile.Name) : "";
    public ICommand TogglePlayPauseCommand { get; }
    public ICommand GoToProfileEditorCommand { get; }
}
