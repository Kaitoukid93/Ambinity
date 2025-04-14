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

    private int _minimumWidth = 2;
    public int MinimumWidth => _minimumWidth;
    private int _minimumHeight = 2;
    public int MinimumHeight => _minimumHeight;

    private void SetItemScale(string value)
    {
        switch (value)
        {
            case "0.25":
            ScaleProperty = 0.25d;
            break;
            case "0.5":
            ScaleProperty =0.5d;
            break;
            case "0.75":
            ScaleProperty =0.75d;
            break;
            case "1":
            ScaleProperty =1d;
            break;
        }
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
        XProperty = PositionAwareItem.X;
        YProperty = PositionAwareItem.Y;
        WidthProperty = PositionAwareItem.Width;
        HeightProperty = PositionAwareItem.Height;
        ScaleProperty = PositionAwareItem.Scale;
        RotationProperty = PositionAwareItem.Rotation;
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
            if (Double.IsNaN(value))
            {
                throw new ArgumentNullException(nameof(XProperty), "Invalid value");
            }
            else
            {
                _xProperty = value;
                PositionAwareItem.SetX((float)XProperty);
                OnPropertyChanged();
            }
        }
    }

    public double YProperty
    {
        get => _yProperty;
        set
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentNullException(nameof(YProperty), "Invalid value");
            }
            else
            {
                _yProperty = value;
                PositionAwareItem.SetY((float)YProperty);
                OnPropertyChanged();
            }
        }
    }

    private double _widthProperty;

    public double WidthProperty
    {
        get => _widthProperty;
        set
        {
            if (Double.IsNaN(value) || value < MinimumWidth)
            {
                throw new ArgumentNullException(nameof(WidthProperty), "Invalid Width");
            }
            else
            {
                _widthProperty = value;
                PositionAwareItem.SetWidth((float)WidthProperty);
                (_canvas.Canvas as Canvas).NeedsRepaint(null);
                OnPropertyChanged();
            }
        }
    }

    private double _heightProperty;

    public double HeightProperty
    {
        get => _heightProperty;
        set
        {
            if (Double.IsNaN(value) || value < MinimumHeight)
            {
                throw new ArgumentNullException(nameof(HeightProperty), "Invalid Height");
            }
            else
            {
                _heightProperty = value;
                PositionAwareItem.SetHeight((float)HeightProperty);
                (_canvas.Canvas as Canvas).NeedsRepaint(null);
                OnPropertyChanged();
            }
        }
    }

    private double _scaleProperty;

    public double ScaleProperty
    {
        get => _scaleProperty;
        set
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentNullException(nameof(ScaleProperty), "Invalid value");
            }
            else
            {
                _scaleProperty = value;
                PositionAwareItem.SetScale((float)ScaleProperty);
                (_canvas.Canvas as Canvas).NeedsRepaint(null);
                OnPropertyChanged();
            }
        }
    }

    private double _rotationProperty;

    public double RotationProperty
    {
        get => _rotationProperty;
        set
        {
            if (Double.IsNaN(value))
            {
                throw new ArgumentNullException(nameof(RotationProperty), "Invalid value");
            }
            else
            {
                _rotationProperty = value;
                PositionAwareItem.SetRotation((float)RotationProperty);
                (_canvas.Canvas as Canvas).NeedsRepaint(null);
                OnPropertyChanged();
            }
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
