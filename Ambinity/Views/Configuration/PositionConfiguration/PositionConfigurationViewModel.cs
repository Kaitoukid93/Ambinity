using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Ambinity.ViewModels;
using Ambinity.Views.Draw2DCanvas;
using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using Draw2D.Core;

namespace Ambinity.Views.Configuration.PositionConfiguration;

/// <summary>
/// UI logic for Dimesion and size, scale.... etc config 
/// </summary>
public class PositionConfigurationViewModel : ViewModelBase
{
    //todo pass params to enable or disable properties such as scale, rotation
    public PositionConfigurationViewModel(Draw2DCanvasViewModel canvas)
    {
        _canvas = canvas;
        SetItemScaleCommand = new RelayCommand<string>(SetItemScale);
    }

    private int _minimumWidth = 5;
    private int _minimumHeight = 5;

    private void SetItemScale(string value)
    {
        ScaleProperty = value;
        TryUpdateItemProperty();
    }

    private Draw2DCanvasViewModel _canvas;

    public void Init(IPositionAware item)
    {
        PositionAwareItem = item;
        CanScale = PositionAwareItem.IsScalable;
        CanRotate = PositionAwareItem.IsRotatable;
        CanResize = PositionAwareItem.IsResizeable;
        ScaleProperty = PositionAwareItem.Scale.ToString("0.00");
        RotationProperty = PositionAwareItem.Rotation.ToString("0.00");
        XProperty = PositionAwareItem.X.ToString("0.00");
        YProperty = PositionAwareItem.Y.ToString("0.00");
        WidthProperty = PositionAwareItem.Width.ToString("0.00");
        HeightProperty = PositionAwareItem.Height.ToString("0.00");
    }

    public void Update()
    {
        if (IsEnabled)
        {
            XProperty = PositionAwareItem.X.ToString("0.00");
            YProperty = PositionAwareItem.Y.ToString("0.00");
            WidthProperty = PositionAwareItem.Width.ToString("0.00");
            HeightProperty = PositionAwareItem.Height.ToString("0.00");
            ScaleProperty = PositionAwareItem.Scale.ToString("0.00");
            RotationProperty = PositionAwareItem.Rotation.ToString("0.00");
        }
    }

    public void TryUpdateItemProperty()
    {
        //check valid
        // var rect = new Rect(float.Parse(XProperty), float.Parse(YProperty), float.Parse(WidthProperty)* float.Parse(ScaleProperty),
        //     float.Parse(HeightProperty)* float.Parse(ScaleProperty));
        //
        // var canvasRect = new Rect(0, 0, _canvas.Canvas.Width, _canvas.Canvas.Height);
        // if (canvasRect.Intersect(rect) != rect)
        // {
        //     return;
        // }
        if (float.Parse(WidthProperty) < _minimumWidth || float.Parse(HeightProperty) < _minimumHeight)
            return;
        PositionAwareItem.X = float.Parse(XProperty);
        PositionAwareItem.Y = float.Parse(YProperty);
        PositionAwareItem.Width = float.Parse(WidthProperty);
        PositionAwareItem.Height = float.Parse(HeightProperty);
        PositionAwareItem.SetRotation(float.Parse(RotationProperty));
        PositionAwareItem.SetScale(float.Parse(ScaleProperty));
        (_canvas.Canvas as Canvas).NeedsRepaint(null);
    }

    private IPositionAware PositionAwareItem;

    /// <summary>
    /// abstraction for item
    /// </summary>
    private string _xProperty;

    private string _yProperty;

    public string XProperty
    {
        get => _xProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(XProperty), "Invalid value");

            _xProperty = value;
            OnPropertyChanged();
        }
    }

    public string YProperty
    {
        get => _yProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(YProperty), "Invalid value");


            _yProperty = value;
            OnPropertyChanged();
        }
    }

    private string _widthProperty;

    public string WidthProperty
    {
        get => _widthProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(WidthProperty), "Invalid value");
            _widthProperty = value;
            OnPropertyChanged();
        }
    }

    private string _heightProperty;

    public string HeightProperty
    {
        get => _heightProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(HeightProperty), "Invalid value");
            _heightProperty = value;
            OnPropertyChanged();
        }
    }

    private string _scaleProperty;

    public string ScaleProperty
    {
        get => _scaleProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(WidthProperty), "Invalid value");
            _scaleProperty = value;
            OnPropertyChanged();
        }
    }

    private string _rotationProperty;

    public string RotationProperty
    {
        get => _rotationProperty;
        set
        {
            decimal numVal = 0;
            var canConvert = decimal.TryParse(value, out numVal);
            if (!canConvert)
                throw new ArgumentException(nameof(RotationProperty), "Invalid value");
            _rotationProperty = value;
            OnPropertyChanged();
        }
    }

    private bool _canRotate;

    public bool CanRotate
    {
        get => _canRotate;
        set
        {
            _canRotate = value;
            OnPropertyChanged();
        }
    }

    private bool _canResize;

    public bool CanResize
    {
        get => _canResize;
        set
        {
            _canResize = value;
            OnPropertyChanged();
        }
    }

    private bool _canScale;

    public bool CanScale
    {
        get => _canScale;
        set
        {
            _canScale = value;
            OnPropertyChanged();
        }
    }

    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }
    private bool _isVisible = true;

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }
    public ICommand SetItemScaleCommand { get; set; }

    public void Reset()
    {
    }
}