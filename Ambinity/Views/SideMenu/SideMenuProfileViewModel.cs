using System.ComponentModel;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfileViewModel : ViewModelBase
{
    public SideMenuProfileViewModel(LightingProfile profile, SideMenuProfileCategoryViewModel category,LightingProfileDecoder decoder)
    {
        Profile = profile;
        Catergory = category;
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        Init();
        CommandSetup();
    }

    private void OnRenderingStatusChanged()
    {
        if(this.Profile != _decoder.CurrentPlayingProfile)
            return;
        IsPlaying = _decoder.CurrentPlayingProfile.IsPlaying;
        
    }

    public LightingProfile Profile { get; set; }
    private string _content = "New Profile";
    private LightingProfileDecoder _decoder;
    public SideMenuProfileCategoryViewModel Catergory { get; set; }

    /// <summary>
    /// Display Name
    /// </summary>
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged();
        }
    }

    private string _icon = "profile";

    /// <summary>
    /// Display Icon
    /// </summary>
    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
           OnPropertyChanged();
        }
    }

    private bool _isSelected;

    /// <summary>
    /// Indicate this profile is selected on side menu 
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

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

    public string ZoneCount => Profile.ZoneCount.ToString();

    #region Methods

    private void Init()
    {
        if (Profile == null)
            return;
        Content = Profile.Name;
        Icon = Profile.Icon;
        IsPlaying = Profile.IsPlaying;
    }

    private void CommandSetup()
    {
        TogglePlayPauseProfileCommand = new RelayCommand(TogglePlayPause);
    }

    private void TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
        if (!IsPlaying)
        {
            _decoder.Stop();
        }
        else
        {
            if (this.Profile == _decoder.CurrentPlayingProfile)
                _decoder.Resume();
            else
            {
                _decoder.Stop();
                _decoder.Init(this.Profile);
            }
        }
    }

    public ICommand TogglePlayPauseProfileCommand { get; set; }

    #endregion
}