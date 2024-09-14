using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Shapes;

namespace Ambinity.Views.AppTour;

public class AppTourElementViewModel
{
    public AppTourElementViewModel(Rect rect, string title, string description)
    {
        Rect = rect;
        Title = title;
        Description = description;
    }

    public AppTourElementViewModel()
    {
    }
    public Rect Rect { get; set; }
    public double Left => (int)Rect.Left;
    public double Top => CalculateTop();
    public string Title { get; set; }
    public string Description { get; set; }
    public ICommand SkipCommand { get; set; }
    public double Height => CalculateHeight();

    private double CalculateHeight()
    {
        if (Rect.Height > maxHeight)
        {
            return Rect.Height;
        }
        return maxHeight;
    }

    private int maxHeight = 200;
    private double CalculateTop()
    {
        if (Rect.Height > maxHeight)
        {
            return Rect.Top;
        }
        else
        {
            return Rect.Top - (maxHeight-Rect.Height)/2;
        }
    }
}