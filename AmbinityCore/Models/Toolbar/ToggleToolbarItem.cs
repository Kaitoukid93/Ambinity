using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class ToggleToolbarItem : ObservableObject, IToolbarItem
{
    public ToggleToolbarItem(string name, string toolTip, string icon)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
    }

    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
    public ICommand Command { get; set; }
    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            _isChecked = value;
            OnPropertyChanged();
        }
    }
}