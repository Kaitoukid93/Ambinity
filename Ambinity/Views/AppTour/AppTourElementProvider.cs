using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Ambinity.Views.AppTour;

public class AppTourElementProvider
{
    private IClassicDesktopStyleApplicationLifetime? _lifeTime;
    private Window? _mainWindow;

    public AppTourElementProvider()
    {
    }

    public AppTourElementViewModel? GetAppTourElements(AppTourElement element, bool isVertical = false,
        bool isReverse = false)
    {
        _lifeTime = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        _mainWindow = _lifeTime.MainWindow;
        if (_mainWindow == null)
            return null;
        if (element.Control == null)
            return null;
        Rect rect = VisualTransformHelper.TransformBoundsTo(element.Control, _mainWindow);
        var elementvm = new AppTourElementViewModel(rect,
            element.Title, element.Description, isVertical, isReverse, new Size(_mainWindow.Width, _mainWindow.Height));
        return elementvm;
    }
}