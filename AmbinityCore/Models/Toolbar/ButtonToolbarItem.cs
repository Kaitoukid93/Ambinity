using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class ButtonToolbarItem : ObservableObject, IToolbarItem
{
    public ButtonToolbarItem(string name, string toolTip, string icon,Color fillColor, ICommand command)
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
        FillColor = fillColor;
        Command = command;
    }

    public ButtonToolbarItem()
    {
        
    }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
    public ICommand Command { get; set; }
    public Color FillColor { get; set; }
}