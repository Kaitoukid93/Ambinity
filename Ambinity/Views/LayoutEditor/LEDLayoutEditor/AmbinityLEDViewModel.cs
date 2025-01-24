using Ambinity.ViewModels;
using AmbinityCore.Models.Device.LED;
using Avalonia.Media;

namespace Ambinity.Views.LayoutEditor.LEDLayoutEditor;

public class AmbinityLEDViewModel : ViewModelBase
{
    private readonly AmbinityLED _led;
    public AmbinityLED LED => _led;

    public AmbinityLEDViewModel(AmbinityLED led)
    {
        _led = led;
        Width = led.Width;
        Height = led.Height;
        Index = led.Index;
        X = led.RelativeX;
        Y = led.RelativeY;
        Geometry = led.Geometry;
    }

    public void SaveIndex()
    {
        _led.Index = Index;
    }
    public void ResetIndex()
    {
        Index = 0;
        IsSelected = false;
        IsIndexVisible = false;
        OnPropertyChanged(nameof(Index));
    }

    public void SetIndex(int index)
    {
        Index = index;
        OnPropertyChanged(nameof(Index));
        IsIndexVisible = true;
    }

    private bool _isIndexVisible = true;
    public bool IsIndexVisible
    {
        get => _isIndexVisible;
        set
        {
            _isIndexVisible = value;
            OnPropertyChanged();
        }
    }
    public float Width { get; set; }
    public float Height { get; set; }
    public int Index { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public string Geometry { get; set; }
    private bool _isSelected;
    private SolidColorBrush _avaloniaColor;

    public SolidColorBrush AvaloniaColor
    {
        get => _avaloniaColor;
        set
        {
            _avaloniaColor = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (!value)
                _led.LED.SetColor(0, 0, 0);
            OnPropertyChanged();
        }
    }

    public void ReloadIndex()
    {
        Index = _led.Index;
        OnPropertyChanged(nameof(Index));
    }

    public void SetColor(Color color)
    {
        _led.LED.SetColor(color.R, color.G, color.B);
        AvaloniaColor = new SolidColorBrush(new Color(255, color.R, color.G, color.B));
    }
}