using System;
using System.Windows.Input;
using Ambinity.Stores;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using Avalonia;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.QuickAccess;

public class ShortcutViewModel : ViewModelBase
{
    private readonly LightingProfileDecoder _decoder;
    private readonly Shortcut _shortcut;
    public event Action<ShortcutViewModel> SelfRemoved;
    public Shortcut Shortcut => _shortcut;

    public ShortcutViewModel(LightingProfileDecoder decoder, Shortcut shortcut,
        QuickAccessNavigationStore navigationStore, ShortcutEditorViewModel shortcutEditorViewModel)
    {
        _shortcutEditorViewModel = shortcutEditorViewModel;
        _navigationStore = navigationStore;
        _decoder = decoder;
        _shortcut = shortcut;
        ShortcutToggleCommand = new RelayCommand(ToggleShortcut, () => CanExecute);
        SelfRemovedCommand = new RelayCommand(SelfRemove);
        GoToShortcutEditorPageCommand = new RelayCommand(GoToShortcutEditorPage);
    }

    private void SelfRemove()
    {
        SelfRemoved?.Invoke(this);
    }

    private void ToggleShortcut()
    {
        //throw new NotImplementedException();
    }

    private void GoToShortcutEditorPage()
    {
        _shortcutEditorViewModel.Init(this);
        _navigationStore.CurrentViewModel = _shortcutEditorViewModel;
    }

    public void Init()
    {
        Name = _shortcut.Name;
        Icon = _shortcut.Icon;
    }

    public void UpdateShortcutProperties()
    {
        _shortcut.Name = Name;
        _shortcut.Icon = Icon;
        _shortcut.LightingProfileID = ShortcutProfileID;
        if (_shortcut.LightingProfileID != null && _shortcut.LightingProfileID != Guid.Empty)
            CanExecute = true;
    }

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private string _icon;
    private readonly QuickAccessNavigationStore _navigationStore;
    private readonly ShortcutEditorViewModel _shortcutEditorViewModel;

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    private bool _canExecute;

    public bool CanExecute
    {
        get => _canExecute;
        set
        {
            _canExecute = value;
            OnPropertyChanged(nameof(ShowWarning));
            OnPropertyChanged();
        }
    }

    private Guid _shortcutProfileID;

    public Guid ShortcutProfileID
    {
        get => _shortcutProfileID;
        set
        {
            _shortcutProfileID = value;
            OnPropertyChanged();
        }
    }

    private bool _isInEditMode;

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set
        {
            _isInEditMode = value;
            OnPropertyChanged(nameof(ShowWarning));
            UpdateItemAppearance();
            OnPropertyChanged();
        }
    }

    private void UpdateItemAppearance()
    {
        if (IsInEditMode)
        {
            ItemWidth = 85;
            ItemCornerRadius = new CornerRadius(4, 4, 4, 4);
        }
        else
        {
            ItemWidth = 60;
            ItemWidth = 50;
            ItemCornerRadius = new CornerRadius(4, 0, 0, 4);
        }
    }

    public bool ShowWarning => !IsInEditMode && !CanExecute;
    public ICommand ShortcutToggleCommand { get; set; }
    public ICommand SelfRemovedCommand { get; set; }
    public ICommand GoToShortcutEditorPageCommand { get; }
    private int _itemWidth = 60;
    private int _itemHeight = 50;

    public int ItemHeight
    {
        get=> _itemHeight;
        set
        {
            _itemHeight = value;
            OnPropertyChanged();
        }
    }

    public int ItemWidth
    {
        get=> _itemWidth;
        set
        {
            _itemWidth = value;
            OnPropertyChanged();
        }
    }
    private CornerRadius _itemCornerradius = new(4, 0, 0, 4);

    public CornerRadius ItemCornerRadius
    {
        get => _itemCornerradius;
        set
        {
            _itemCornerradius = value;
            OnPropertyChanged();
        }
    }
}