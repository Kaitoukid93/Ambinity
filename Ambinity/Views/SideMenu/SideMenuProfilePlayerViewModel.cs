using System;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using Ambinity.Views.Root;
using AmbinityCore.Models.Profile;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.SideMenu;

public class SideMenuProfilePlayerViewModel : ViewModelBase
{
    public event Action<LightingProfile> PlayingButtonClicked;
    public SideMenuProfilePlayerViewModel(LightingProfileDecoder decoder, RootNavigationStores rootNavigationStores)
    {
        _decoder = decoder;
        _decoder.RenderingStatusChanged += OnRenderingStatusChanged;
        CurrentPlayingProfile = _decoder.CurrentPlayingProfile;
        IsPlaying = _decoder.IsRendering;
        _rootNavigationStores = rootNavigationStores;
        GoToProfileEditorCommand = new RelayCommand(GotoProfileEditor);
    }

    private RootNavigationStores _rootNavigationStores;
    private void OnRenderingStatusChanged()
    {
        
        IsPlaying = _decoder.IsRendering;
        CurrentPlayingProfile = _decoder.CurrentPlayingProfile;
        
    }

    private void GotoProfileEditor()
    {
        if(CurrentPlayingProfile ==null)
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
    public ICommand GoToProfileEditorCommand { get; set; }
}