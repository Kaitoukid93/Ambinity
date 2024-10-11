using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;

namespace Ambinity.QuickAccess;

public class ShortcutViewModel : ViewModelBase
{
    private readonly LightingProfileDecoder _decoder;
    private readonly Shortcut _shortcut;

    public ShortcutViewModel(LightingProfileDecoder decoder, Shortcut shortcut)
    {
        _decoder = decoder;
        _shortcut = shortcut;
        
    }

    public void Init()
    {
        Name = _shortcut.Name;
        Icon = _shortcut.Icon;
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

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }
    public ICommand ShortcutToggleCommand { get; set; }
}