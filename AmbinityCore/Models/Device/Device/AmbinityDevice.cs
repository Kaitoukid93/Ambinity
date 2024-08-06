using System.Collections.ObjectModel;
using AmbinityCore.Enums;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Models.Geography;
using AmbinityCore.Utils;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core;
using Draw2D.Core.Constants;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Shapes.FigureExtensions;
using Newtonsoft.Json;
using SkiaSharp;
using RGBLEDOrderEnum = AmbinityCore.Enums.RGBLEDOrderEnum;

namespace AmbinityCore.Models.Device;

public class AmbinityDevice : ObservableObject, IPositionAware
{
    public event Action DeviceUpdate;
    public event Action<Rect> TryUpdateEvent;

    public AmbinityDevice()
    {
        Leds = new ObservableCollection<AmbinityLED>();
    }

    /// <summary>
    /// construct new Ambinity device from existed layout
    /// </summary>
    /// <param name="layout"></param>
    public AmbinityDevice(AmbinityDeviceLayout layout)
    {
        Layout = layout;
        Leds = new ObservableCollection<AmbinityLED>();
        LoadLayout();
    }

    private string _deviceName = "New Slave Device";

    /// <summary>
    /// Display name of the device
    /// </summary>
    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    private string _deviceDescription = "";

    /// <summary>
    /// Describe the function and look of the device
    /// </summary>
    public string DeviceDescription
    {
        get => _deviceDescription;
        set => SetProperty(ref _deviceDescription, value);
    }

    private List<DeviceType> _targetParrentDeviceType = new List<DeviceType>() { DeviceType.Unknown };

    /// <summary>
    /// device type this slave is compatible with
    /// </summary>
    public List<DeviceType> TargetParrentDeviceType
    {
        get => _targetParrentDeviceType;
        set => SetProperty(ref _targetParrentDeviceType, value);
    }

    private RGBLEDOrderEnum _rgbOrder;

    /// <summary>
    /// 
    /// </summary>
    public RGBLEDOrderEnum RGBOrder
    {
        get => _rgbOrder;
        set => SetProperty(ref _rgbOrder, value);
    }

    /// <summary>
    /// All the leds this device contains
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<AmbinityLED> Leds { get; set; }

    /// <summary>
    /// Layout data
    /// </summary>
    public AmbinityDeviceLayout Layout { get; set; }


    #region Iposition aware implement

    private bool _isDragable = true;
    private bool _isSelectable = true;
    private bool _isDeleteable = true;
    private bool _isResizeable = true;
    private bool _isHitTestVisible = true;
    private bool _isRotatable;
    private bool _isDraggable;
    private bool _isScalable;
    public string Name => _deviceName;

    /// <summary>
    /// Device can or can not be selected on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsSelectable
    {
        get => _isSelectable;
        set => SetProperty(ref _isSelectable, value);
    }

    /// <summary>
    /// Device can or can not be deleted from the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsDeleteable
    {
        get => _isDeleteable;
        set => SetProperty(ref _isDeleteable, value);
    }

    /// <summary>
    /// Device can or can not be resized on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsResizeable
    {
        get => _isResizeable;
        set => SetProperty(ref _isResizeable, value);
    }

    /// <summary>
    /// Device can or can not be drag on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsDraggable
    {
        get => _isDraggable;
        set => SetProperty(ref _isDraggable, value);
    }

    /// <summary>
    /// Device can or can not be rotate on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsRotatable
    {
        get => _isRotatable;
        set => SetProperty(ref _isRotatable, value);
    }

    /// <summary>
    /// Device can or can not be scale on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsScalable
    {
        get => _isScalable;
        set => SetProperty(ref _isScalable, value);
    }
    public ContainerFigure GetContainer()
    {
        return new DeviceContainerFigure(X, Y, Width, Height)
        {
            IsResizable = this.IsResizeable,
            IsSelectable = this.IsSelectable,
            IsDragable = this.IsDraggable,
        };
    }

    public ContainerFigure Clone(float X, float Y)
    {
        throw new NotImplementedException("Can not clone deivce");
    }
    #endregion

    #region Canvas Corordinate Properties

    /// <summary>
    /// Device absolute position on the canvas respect ot top left corner
    /// </summary>
    private float _x = 0;

    private float _y = 0;

    public float X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

    private float _width = 100;
    private float _height = 100;

    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

    /// <summary>
    /// set scale ot match canvas size
    /// </summary>
    private float _scale = 0.25f;

    private float _rotation = 0;

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            OnPropertyChanged();
            DeviceUpdate?.Invoke();
        }
    }

   

    #endregion

    /// <summary>
    /// Device can or can not be hovered on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsHitTestVisible
    {
        get => _isHitTestVisible;
        set => SetProperty(ref _isHitTestVisible, value);
    }

    /// <summary>
    /// Lock the setup when modifying
    /// </summary>
    [JsonIgnore]
    public object Lock { get; } = new object();

    /// <summary>
    /// White balance
    /// </summary>
    public byte RedScale { get; set; }

    public byte GreenScale { get; set; }
    public byte BlueScale { get; set; }


    #region Methods

    public void SetScale(float scale)
    {
        Scale = scale;
        DeviceUpdate?.Invoke();
    }

    public void SetRotation(float angle)
    {
        Rotation = angle;
        DeviceUpdate?.Invoke();
    }

    public void LoadLayout()
    {
        Layout.ApplyToDevice(this);
    }

    public void UpdateSizeByChild(bool withPoint)
    {
        //get all child and set size
        var rects = new List<Rect>();
        foreach (var led in Leds)
        {
            rects.Add(led.RelativeRectangle);
        }

        var newBound = RectCalculation.GetBound(rects.ToArray());
        Width = (float)newBound.Width;
        Height = (float)newBound.Height;
        if (withPoint)
        {
            X = (float)newBound.Left;
            Y = (float)newBound.Top;
        }
    }

    #endregion
}