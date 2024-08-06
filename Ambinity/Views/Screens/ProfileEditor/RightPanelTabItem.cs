using CommunityToolkit.Mvvm.ComponentModel;

namespace Ambinity.Views.Screens.ProfileEditor;

public class RightPanelTabItem : ObservableObject
{
    public RightPanelTabItem(string name, string toolTip, string icon)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
    }

    private bool _isSelected;
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}