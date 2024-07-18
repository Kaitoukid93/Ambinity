using AmbinityCore.Models.Flyout;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class FlyoutButtonToolbarItem : ObservableObject, IToolbarItem
{
    public FlyoutButtonToolbarItem(string name, string toolTip, string icon)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
        FlyoutItems = new List<IFlyoutItem>();
    }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
    public List<IFlyoutItem> FlyoutItems { get; set; }

}