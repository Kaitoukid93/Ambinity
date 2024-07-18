using System.Collections.ObjectModel;
using AmbinityCore.Enums;
using AmbinityCore.Models.Device.LED;
using AmbinityCore.Models.GeneralSetting;
using AmbinityCore.Utils;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Draw2D.Core;
using Draw2D.Core.Constants;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Shapes.FigureExtensions;
using SkiaSharp;

namespace AmbinityCore.Models.Device;

public class AmbinityDevice : ObservableObject
{
    public event Action DeviceUpdate;

    public AmbinityDevice()
    {
        Leds = new ObservableCollection<AmbinityLED>();
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

    /// <summary>
    /// All the leds this device contains
    /// </summary>
    public ObservableCollection<AmbinityLED> Leds { get; set; }

    /// <summary>
    /// Layout data
    /// </summary>
    public AmbinityDeviceLayout Layout { get; set; }

    #region Canvas Behavior Properties

    private bool _isDragable = true;
    private bool _isSelectable = true;
    private bool _isDeleteable = true;
    private bool _isResizeable = true;
    private bool _isHitTestVisible = true;

    /// <summary>
    /// Device can or can not be drag on the canvas
    /// </summary>
    public bool IsDragable
    {
        get => _isDragable;
        set => SetProperty(ref _isDragable, value);
    }

    /// <summary>
    /// Device can or can not be selected on the canvas
    /// </summary>
    public bool IsSelectable
    {
        get => _isSelectable;
        set => SetProperty(ref _isSelectable, value);
    }

    /// <summary>
    /// Device can or can not be deleted from the canvas
    /// </summary>
    public bool IsDeleteable
    {
        get => _isDeleteable;
        set => SetProperty(ref _isDeleteable, value);
    }

    /// <summary>
    /// Device can or can not be resized on the canvas
    /// </summary>
    public bool IsResizeable
    {
        get => _isResizeable;
        set => SetProperty(ref _isResizeable, value);
    }

    /// <summary>
    /// Device can or can not be hovered on the canvas
    /// </summary>
    public bool IsHitTestVisible
    {
        get => _isHitTestVisible;
        set => SetProperty(ref _isHitTestVisible, value);
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
        set => SetProperty(ref _x, value);
    }

    public float Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    private float _width = 100;
    private float _height = 100;

    public float Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public float Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private float _scale = 1;
    private float _rotation = 0;

    public float Scale
    {
        get => _scale;
        set => SetProperty(ref _scale, value);
    }

    public float Rotation
    {
        get => _rotation;
        set => SetProperty(ref _rotation, value);
    }
    #endregion


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