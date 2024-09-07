using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace AmbinityCore.Models.Flyout;

public class FlyoutItem : IFlyoutItem
{
    public event Action<FlyoutItem> FlyoutItemSelected;
    public FlyoutItem(string content, string icon)
    {
        Content = content;
        Icon = icon;
        Command = new RelayCommand(Selected);
    }

    private void Selected()
    {
        FlyoutItemSelected?.Invoke(this);
    }

    public string Content { get; set; }
    public string Icon { get; set; }
    public ICommand Command { get; set; }
}