using System.Windows.Input;
using Ambinity.ViewModels;
using Avalonia;
using Avalonia.Controls.Shapes;

namespace Ambinity.Views.AppTour;

public class AppTourElementViewModel : ViewModelBase
{
    public AppTourElementViewModel(Rect rect, string title, string description, bool isVertical, bool isReverse, Size windowSize)
    {
        Rect = rect;
        Title = title;
        Description = description;
        IsVertical = isVertical;
        IsReverse = isReverse;
        _windowSize = windowSize;
    }

    public AppTourElementViewModel()
    {
    }

    public bool IsVertical { get; set; }
    public bool IsReverse { get; set; }
    public Rect Rect { get; set; }
    public double Left => CalculateLeft();
    public double Top => CalculateTop();
    public string Title { get; set; }
    public string Description { get; set; }
    public ICommand SkipCommand { get; set; }
    private string _skipButtonContent = "Skip";

    public string SkipButtonContent
    {
        get => _skipButtonContent;
        set
        {
            _skipButtonContent = value;
            OnPropertyChanged();
        }
    }
    public double Height => CalculateHeight();
    private Size _windowSize;

    private double CalculateHeight()
    {
        if (IsVertical)
            return maxContentHeight + separatorLength + Rect.Height;
        if (Rect.Height > maxContentHeight)
        {
            return Rect.Height;
        }

        return maxContentHeight;
    }

    private int separatorLength = 200;
    private int maxContentWidth = 300;
    private int maxContentHeight = 242;

    private double CalculateTop()
    {
        if (IsVertical)
        {
            return Rect.Bottom - Height < 20 ? Rect.Bottom + Height : Rect.Bottom - Height;
        }

        if (Rect.Height > maxContentHeight)
        {
            return Rect.Top;
        }

        return Rect.Top - (maxContentHeight - Rect.Height) / 2;
    }

    private double CalculateLeft()
    {
        if (IsVertical)
        {
            var offset = Rect.Center.X > maxContentWidth / 2 ? Rect.Center.X - maxContentWidth / 2 : 0;
            return offset;
        }
        else
        {
            var offset = Rect.Right + maxContentWidth > _windowSize.Width  ? Rect.Left - maxContentWidth - separatorLength -20 : Rect.Left;

            return offset;
        }
    }
}