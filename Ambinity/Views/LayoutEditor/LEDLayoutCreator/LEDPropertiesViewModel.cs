using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.LayoutEditor.LEDLayoutEditor;
using AmbinityCore.Models.Device.LED;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;
using OpenRGB.NET;
using Serilog;


namespace Ambinity.Views.LayoutEditor.LEDLayoutCreator;

public class LEDPropertiesViewModel : ViewModelBase

{
    public LEDPropertiesViewModel()
    {
        LEDs = [];
    }

    public void Init(IEnumerable<Figure> leds)
    {

        if (leds == null || leds.Count() == 0)
        {
            IsValid = false;
            return;
        }
        IsValid = true;
        _leds = [];
        foreach (var led in leds)
        {
            _leds.Add(led);
        }
        if (_leds.Count == 1)
        {
            Width = _leds[0].Width;
            Height = _leds[0].Height;
            X = _leds[0].X; ;
            Y = _leds[0].Y; ;
        }
        else
        {

        }

        UpdateGeometry();
    }
    private int _startIndex;
    private float _x;
    private float _y;
    private float _width;
    private float _height;
    private bool _isReverseSelected;
    private ObservableCollection<Figure> _leds;
    public ObservableCollection<LEDContainerFigureViewModel> LEDs { get; set; }
    public bool IsMultipleLEDSelected => LEDs.Count > 1;
    private double _geometryWidth;
    private double _geometryHeight;
    public double GeometryWidth
    {
        get => _geometryWidth; set
        {
            _geometryWidth = value;
            OnPropertyChanged();
        }
    }
    public double GeometryHeight
    {
        get => _geometryHeight;
        set
        {
            _geometryHeight = value;
            OnPropertyChanged();
        }
    }
    public bool IsReverseSelected
    {
        get => _isReverseSelected;
        set
        {
            _isReverseSelected = value;
            OnPropertyChanged();
        }
    }
    private void UpdateGeometry()
    {
        LEDs.Clear();
        if (_leds == null || _leds.Count == 0)
            return;
        var minX = _leds.Min(f => f.X);
        var minY = _leds.Min(f => f.Y);
        var maxX = _leds.Max(f => f.X + f.Width);
        var maxY = _leds.Max(f => f.Y + f.Height);
        var boundingBox = new Rect(minX, minY, maxX - minX, maxY - minY);
        GeometryWidth = boundingBox.Width;
        GeometryHeight = boundingBox.Height;
        X = (float)boundingBox.X;
        Y = (float)boundingBox.Y;
        Width = (float)boundingBox.Width;
        Height = (float)boundingBox.Height;
        foreach (var led in _leds)
        {
            if (led is not LEDContainerFigure)
                continue;
            var vm = new LEDContainerFigureViewModel(led as LEDContainerFigure, (float)boundingBox.X, (float)boundingBox.Y);
            LEDs.Add(vm);
        }
        if (LEDs.Count == 1)
        {
            StartIndex = LEDs[0].Index ?? 0;
        }
        OnPropertyChanged(nameof(IsMultipleLEDSelected));
    }
    private Geometry _geometry;
    public Geometry Geometry
    {
        get => _geometry;
        set
        {
            _geometry = value;
            OnPropertyChanged();
        }
    }

    private bool _isValid;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            _isValid = value;
            OnPropertyChanged();
        }
    }
    public ICommand SetIndexCommand => new RelayCommand(() =>
     {
         SetIndex();
     });

    private void SetIndex()
    {

        if (StartIndex < 0)
            return;
        int count = StartIndex;
        if (IsReverseSelected)
        {
            count = LEDs.Count + StartIndex - 1;
        }

        foreach (var led in LEDs.OrderBy(l => l.X).ThenBy(l=>l.Y))
        {
            if (IsReverseSelected)
                led.SetIndex(count--);
            else
                led.SetIndex(count++);

        }
        LEDs[0].Repaint();
    }
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged();
        }
    }
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
        }
    }
    public int StartIndex
    {
        get => _startIndex;
        set
        {
            _startIndex = value;
            OnPropertyChanged();
        }
    }

}
