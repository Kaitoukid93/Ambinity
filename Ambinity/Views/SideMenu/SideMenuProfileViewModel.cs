using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfileViewModel : ViewModelBase
{
    public SideMenuProfileViewModel(LightingProfile profile,SideMenuProfileCategoryViewModel category)
    {
        Profile = profile;
        Catergory = category;
        Init();
        CommandSetup();
    }

    public LightingProfile Profile { get; set; }
    private string _content = "New Profile";
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
            RaisePropertyChanged(nameof(Content));
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
            RaisePropertyChanged(nameof(Icon));
        }
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            RaisePropertyChanged(nameof(IsPlaying));
        }
    }
    public string ZoneCount => Profile.ZoneCount.ToString();

    #region Methods

    private void Init()
    {
        if(Profile==null)
            return;
        Content = Profile.Name;
        Icon = Profile.Icon;
    }

    private void CommandSetup()
    {
        TogglePlayPauseProfileCommand = new RelayCommand(TogglePlayPause);
    }

    private void TogglePlayPause()
    {
        IsPlaying = !IsPlaying;
    }

    public ICommand TogglePlayPauseProfileCommand { get; set; }

    #endregion
}