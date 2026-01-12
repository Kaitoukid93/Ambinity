using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class ButtonToolbarItem : ObservableObject, IToolbarItem
{
    public ButtonToolbarItem(string name, string toolTip, string icon,SolidColorBrush fillColor, ICommand command,string disabledToolTip = "Tool is disabled while rendering")
    {
        Name = name;
        ToolTip = toolTip;
        Icon = icon;
        FillColor = fillColor;
        DisabledToolTip = disabledToolTip;
        Command = command;

    }

    public ButtonToolbarItem()
    {

    }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string DisabledToolTip {get;set;}
    public string Icon { get; set; }
    public ICommand Command { get; set; }
    public SolidColorBrush FillColor { get; set; }
}
