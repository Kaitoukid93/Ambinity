using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Ambinity.Views.AppTour;

public static class AppTourElementHelper
{
    
   
    public static AppTourElementViewModel? GetAppTourElements(AppTourElement element, Window? mainWindow = null)
    {
        if (mainWindow == null)
            return null;
        if (element.Control == null)
            return null;
        Rect rect = VisualTransformHelper.TransformBoundsTo(element.Control, mainWindow);
        var elementvm = new AppTourElementViewModel(rect,
            element.Title, element.Description);
        return elementvm;

    }
}