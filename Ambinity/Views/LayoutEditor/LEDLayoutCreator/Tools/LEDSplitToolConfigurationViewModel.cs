using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ambinity.ViewModels;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Utils;
using CommunityToolkit.Mvvm.Input;

namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class LEDSplitToolConfigurationViewModel : ViewModelBase
{

    public event Action Cancel;
    public event Action Accept;
    private ObservableCollection<LEDContainerFigureViewModel> _leds;
    public ObservableCollection<LEDContainerFigureViewModel> Leds
    {
        get => _leds;
        set
        {
            _leds = value;
            OnPropertyChanged();
        }
    }
    private LEDContainerFigure _led;
    public double GeometryWidth { get; set; }
    public double GeometryHeight { get; set; }
    private float _offsetX;
    private float _offsetY;

    public LEDSplitToolConfigurationViewModel(LEDContainerFigure led)
    {
        _led = led;
        _offsetX = _led.X;
        _offsetY = _led.Y;
        GeometryHeight = led.Height;
        GeometryWidth = led.Width;
        Leds = new ObservableCollection<LEDContainerFigureViewModel>() { new LEDContainerFigureViewModel(led, _offsetX, _offsetY) };
        UpdatePreview();
        AcceptCommand = new RelayCommand(OnUserAccept);
        CancelCommand = new RelayCommand(OnUserCancel);
    }

    private void OnUserCancel()
    {
        Cancel?.Invoke();
    }

    private void OnUserAccept()
    {
        Accept?.Invoke();
    }

    private int _rowNumber = 2;
    private int _columnNumber = 2;
    private float _rowHeight;
    private float _columnWidth;
    private float _rowGutter = 2;
    private float _columnGutter = 2;
    private float _totalHeight;
    private float _totalWidth;

    public int RowNumber
    {
        get => _rowNumber;
        set
        {
            _rowNumber = value;
            OnPropertyChanged();
            UpdatePreview();
        }
    }
    public int ColumnNumber
    {
        get => _columnNumber;
        set
        {
            _columnNumber = value;
            OnPropertyChanged();
            UpdatePreview();
        }
    }
    public float RowHeight
    {
        get => _rowHeight;
        set
        {
            _rowHeight = value;
            OnPropertyChanged();

        }
    }
    public float ColumnWidth
    {
        get => _columnWidth;
        set
        {
            _columnWidth = value;
            OnPropertyChanged();
        }
    }
    public float RowGutter
    {
        get => _rowGutter;
        set
        {
            _rowGutter = value;
            OnPropertyChanged();
            UpdatePreview();
        }
    }
    public float ColumnGutter
    {
        get => _columnGutter;
        set
        {
            _columnGutter = value;
            OnPropertyChanged();
            UpdatePreview();
        }
    }
    public float TotalHeight
    {
        get => _totalHeight;
        set
        {
            _totalHeight = value;
        }
    }
    public float TotalWidth
    {
        get => _totalWidth;
        set
        {
            _totalWidth = value;
        }
    }
    public RelayCommand AcceptCommand { get; }
    public ICommand CancelCommand { get; }

    private void UpdatePreview()
    {
        var result = GeometryUltilities.SplitLEDInToMatrix(_led, RowNumber, ColumnNumber, ColumnGutter, RowGutter);
        if (result != null)
        {
            Leds.Clear();
            foreach (var led in result)
            {
                var vm = new LEDContainerFigureViewModel(led as LEDContainerFigure, _offsetX, _offsetY);
                Leds.Add(vm);
            }
        }

    }


}
public class LEDContainerFigureViewModel
{
    public LEDContainerFigureViewModel(LEDContainerFigure figure, float offsetX, float offsetY)
    {
        X = figure.X - offsetX;
        Y = figure.Y - offsetY;
        _figure = figure;
        Width = figure.Width;
        Height = figure.Height;
        Geometry = (figure.ChildItem as AmbinityLED).Geometry;
        Index = (figure.ChildItem as AmbinityLED).Index;

    }
    private LEDContainerFigure _figure;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public string Geometry { get; set; }
    public int? Index { get; set; }

    public void SetIndex(int index)
    {
        var led = _figure.ChildItem as AmbinityLED;
        if (led != null)
        {
            led.Index = index;
        }

    }
    public void Repaint()
    {
        _figure?.Canvas.NeedsRepaint(_figure);
    }

}
