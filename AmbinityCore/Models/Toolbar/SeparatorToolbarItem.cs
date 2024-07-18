using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public class SeparatorToolbarItem : ObservableObject, IToolbarItem
{
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string Icon { get; set; }
}