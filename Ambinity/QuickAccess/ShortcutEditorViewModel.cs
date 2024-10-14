using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class ShortcutEditorViewModel : ViewModelBase
{
    public ShortcutEditorViewModel(LightingProfileRepository lightingProfileRepository,QuickAccessNavigationStore navigationStore)
    {
        _navigationStore = navigationStore;
        _lightingProfileRepository = lightingProfileRepository;
        AvailableIcons = new List<string>();
        var dicts = Application.Current.Resources.MergedDictionaries;
        var geometriesDict = dicts[1] as ResourceDictionary;
        foreach (var key in geometriesDict.Keys)
        {
            AvailableIcons.Add(key as string);
        }

        ApplySettingsCommand = new RelayCommand(ApplySettings);
    }

    public ICommand ApplySettingsCommand { get; set; }

    private void ApplySettings()
    {
        ShortcutViewModel.Icon = _icon;
        ShortcutViewModel.ShortcutProfileID = _selectedProfile.ID;
        ShortcutViewModel.UpdateShortcutProperties();
        _navigationStore.GoBack();
    }

    public List<string> AvailableIcons { get; set; }
    public List<LightingProfile> AvailableLightingProfiles { get; set; } = new List<LightingProfile>();

    public void Init(ShortcutViewModel shortcut)
    {
        AvailableLightingProfiles.Clear();
        foreach (var item in _lightingProfileRepository.Items)
        {
            AvailableLightingProfiles.Add(item as LightingProfile);
        }

        ShortcutViewModel = shortcut;
        Icon = AvailableIcons.Where(i => i == shortcut.Icon).FirstOrDefault();
        _selectedProfile =
            ShortcutViewModel.Shortcut.LightingProfileID == null ||
            ShortcutViewModel.Shortcut.LightingProfileID == Guid.Empty
                ? AvailableLightingProfiles.First()
                : AvailableLightingProfiles.Where(p => p.ID == ShortcutViewModel.Shortcut.LightingProfileID)
                    .FirstOrDefault();
    }

    private ShortcutViewModel _shortcutViewModel;

    public ShortcutViewModel ShortcutViewModel
    {
        get => _shortcutViewModel;
        set
        {
            _shortcutViewModel = value;
            OnPropertyChanged();
        }
    }

    private string _icon;
    private readonly LightingProfileRepository _lightingProfileRepository;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    private LightingProfile _selectedProfile;
    private readonly QuickAccessNavigationStore _navigationStore;

    public LightingProfile SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnPropertyChanged();
        }
    }

}