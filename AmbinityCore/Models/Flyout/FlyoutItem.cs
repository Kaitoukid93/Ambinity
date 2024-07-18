using System.Windows.Input;

namespace AmbinityCore.Models.Flyout;

public class FlyoutItem : IFlyoutItem
{
    public FlyoutItem(string content, string icon)
    {
        Content = content;
        Icon = icon;
    }
    public string Content { get; set; }
    public string Icon { get; set; }
    public ICommand Command { get; set; }
}