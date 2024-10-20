using System.ComponentModel;

namespace AmbinityCore.Models.Toolbar;

public interface IToolbarItem: INotifyPropertyChanged
{
    string Name { get; set; }
    string ToolTip { get; set; }
    string DisabledToolTip { get; }
    string Icon { get; set; }
    
}