using Avalonia.Media;

namespace Ambinity.Installer.ViewModels;

public class AmbinityImageViewModel : ViewModelBase
{
    public AmbinityImageViewModel(SolidColorBrush brush)
    {
      AccentBrush = brush;
    }
    public SolidColorBrush AccentBrush { get; set; }
}