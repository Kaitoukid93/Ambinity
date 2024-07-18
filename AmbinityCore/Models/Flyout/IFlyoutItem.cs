using System.Windows.Input;

namespace AmbinityCore.Models.Flyout;

public interface IFlyoutItem
{
    string Content { get; set; }
    string Icon { get; set; }
    ICommand Command { get; set; }
    
}