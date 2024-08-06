using AmbinityCore.Repositories;
using Avalonia.Media;

namespace Ambinity.Views.Configuration.ColorConfiguration.Parameters;

public class StaticColorSelectionParameterViewModel : ParameterViewModelBase
{
    public StaticColorSelectionParameterViewModel(StaticColor color)
    {
        Color = color.GetBrush();
    }

    private Brush _color;
    public Brush Color
    {
        get => _color;
        set
        {
            _color = value;
            OnPropertyChanged();
        }
    }
}