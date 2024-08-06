using System.Text;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.SideMenu;
using AmbinityCore.Models.Profile;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.Screens.ProfileEditor;

public class ZoneMappingRenderControllerViewModel : ViewModelBase
{
    public ZoneMappingRenderControllerViewModel(LightingProfileDecoder profileDecoder, SideMenuViewModel sideMenu)
    {
        TogglePlayCommand = new RelayCommand(TogglePlay);
        _decoder = profileDecoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        _sideMenu = sideMenu;
        _sideMenu.SelectedProfileChanged += OnSelectedProfileChanged;
    }
    private void OnRenderingStatusChanged()
    {
        if(CurrentSelectedProfile != _decoder.CurrentPlayingProfile)
            return;
        IsPlaying = _decoder.CurrentPlayingProfile.IsPlaying;
        
    }
    private void OnSelectedProfileChanged(SideMenuProfileViewModel profile)
    {
        if (profile.Profile != _decoder.CurrentPlayingProfile)
        {
            CurrentSelectedProfile = profile.Profile;
            IsPlaying = false;
        }
    }

    private LightingProfileDecoder _decoder;
    private SideMenuViewModel _sideMenu;
    private LightingProfile _currentPlayingProfile;

    public LightingProfile CurrentSelectedProfile
    {
        get => _currentPlayingProfile;
        set
        {
            _currentPlayingProfile = value;
            OnPropertyChanged();
        }
    }

    public void Init(LightingProfile profile)
    {
        CurrentSelectedProfile = profile;
        IsPlaying = CurrentSelectedProfile.IsPlaying;
    }

    private void TogglePlay()
    {
        IsPlaying = !IsPlaying;
        if (!IsPlaying)
        {
            _decoder.Stop();
        }
        else
        {
            if (CurrentSelectedProfile == _decoder.CurrentPlayingProfile)
                _decoder.Resume();
            else
            {
                _decoder.Stop();
                _decoder.Init(CurrentSelectedProfile);
            }
        }
    }

    /// <summary>
    /// get rendering status
    /// </summary>
    public bool IsRendering => _decoder.IsRendering;

    private bool _isPlaying;

    /// <summary>
    /// Indicate this profile is playing or not
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    public ICommand TogglePlayCommand { get; set; }
}