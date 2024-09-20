using System.Windows.Input;
using AmbinityCore.Models.Flyout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class FlyoutButtonToolbarItem : ObservableObject, IToolbarItem
{
    public FlyoutButtonToolbarItem(string name, string toolTip, string icon, Color fillColor, ICommand command)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
        FlyoutItems = new List<IFlyoutItem>();
        Command = command;
        FillColor = fillColor;
    }

    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
    public List<IFlyoutItem> FlyoutItems { get; set; }
    public ICommand Command { get; set; }
    public Color FillColor { get; set; }
}