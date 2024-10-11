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
        ScaleProperty = double.Parse(value);
        TryUpdateItemProperty();
    }

    private Draw2DCanvasViewModel _canvas;

    public void Init(IPositionAware item)
    {
        PositionAwareItem = item;
        CanScale = PositionAwareItem.IsScalable;
        CanRotate = PositionAwareItem.IsRotatable;
        CanResize = PositionAwareItem.IsResizeable;
        ScaleProperty = PositionAwareItem.Scale;
        RotationProperty = PositionAwareItem.Rotation;
        XProperty = PositionAwareItem.X;
        YProperty = PositionAwareItem.Y;
        WidthProperty = PositionAwareItem.Width;
        HeightProperty = PositionAwareItem.Height;
    }

    public void Update()
    {
        if (IsEnabled)
        {
            XProperty = PositionAwareItem.X;
            YProperty = PositionAwareItem.Y;
            WidthProperty = PositionAwareItem.Width;
            HeightProperty = PositionAwareItem.Height;
            ScaleProperty = PositionAwareItem.Scale;
            RotationProperty = PositionAwareItem.Rotation;
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
        if ((float)(WidthProperty) < _minimumWidth || (float)HeightProperty < _minimumHeight)
            return;
        PositionAwareItem.X = (float)XProperty;
        PositionAwareItem.Y =(float)YProperty;
        PositionAwareItem.Width = (float)WidthProperty;
        PositionAwareItem.Height = (float)HeightProperty;
        PositionAwareItem.SetRotation((float)RotationProperty);
        PositionAwareItem.SetScale((float)ScaleProperty);
        (_canvas.Canvas as Canvas).NeedsRepaint(null);
    }

    private IPositionAware PositionAwareItem;

    /// <summary>
    /// abstraction for item
    /// </summary>
    private double _xProperty;

    private double _yProperty;

    public double XProperty
    {
        get => _xProperty;
        set
        {

            _xProperty = value;
            OnPropertyChanged();
        }
    }

    public double YProperty
    {
        get => _yProperty;
        set
        {
            _yProperty = value;
            OnPropertyChanged();
        }
    }

    private double _widthProperty;

    public double WidthProperty
    {
        get => _widthProperty;
        set
        {
            _widthProperty = value;
            OnPropertyChanged();
        }
    }

    private double _heightProperty;

    public double HeightProperty
    {
        get => _heightProperty;
        set
        {
            
            _heightProperty = value;
            OnPropertyChanged();
        }
    }

    private double _scaleProperty;

    public double ScaleProperty
    {
        get => _scaleProperty;
        set
        {
          
            _scaleProperty = value;
            OnPropertyChanged();
        }
    }

    private double _rotationProperty;

    public double RotationProperty
    {
        get => _rotationProperty;
        set
        {
            
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