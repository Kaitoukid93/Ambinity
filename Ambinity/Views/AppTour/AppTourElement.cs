using Avalonia.Controls;

namespace Ambinity.Views.AppTour;

public class AppTourElement
{
    public AppTourElement(Control control, string title, string description)
    {
        Control = control;
        Title = title;
        Description = description;
    }
    public Control? Control { get; set; }
    public string Title { get; set; }
    public string Description { get; set; } 
}