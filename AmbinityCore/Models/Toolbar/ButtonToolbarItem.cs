using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class ButtonToolbarItem : ObservableObject, IToolbarItem
{
    public ButtonToolbarItem(string name, string toolTip, string icon)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
    }

    public ButtonToolbarItem()
    {
        
    }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
    public ICommand Command { get; set; }
}