using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using AmbinityCore.Models.Geography;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Draw2D.Core;
using SkiaSharp;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;
using AmbinityCore.Helpers;
namespace AmbinityCore.Models.Device.LED;

public class AmbinityLED : ObservableObject, IPositionAware
{
    /// <summary>
    /// extend ARGBLED with size and location on a real device
    /// </summary>
    public AmbinityLED(ArgbLed led, AmbinityDevice device,
        float left,
        float top,
        float width,
        float height,
        int? index,
        bool isActivated,
        string geometry)
    {
        LED = led;
        Device = device;

        LedID = index;
        Width = width;
        Height = height;
        RelativeX = left;
        RelativeY = top;
        Geometry = geometry;
        Index = index;
        Name = "LED " + (index ?? 0);
    }

    public ArgbLed LED { get; }
    public int? Index { get; set; }
    [JsonIgnore] public AmbinityDevice Device { get; }

    /// <summary>
    /// Get rectangle relative positioned to the device
    /// </summary>
    public Rect RelativeRectangle => new Rect(RelativeX, RelativeY, Width, Height);

    /// <summary>
    /// Get rectangle absolute positioned to the device
    /// </summary>
    public Rect AbsoluteRectangle => new Rect(AbsoluteX, AbsoluteY, Width, Height);


    private string _geometry;

    /// <summary>
    /// Path Geometry of this LED
    /// </summary>
    public string Geometry
    {
        get => _geometry;
        set => SetProperty(ref _geometry, value);
    }

    private int? _ledID = 0;

    /// <summary>
    /// LED ID map to real world
    /// </summary>
    public int? LedID
    {
        get => _ledID;
        set => SetProperty(ref _ledID, value);
    }



    public Avalonia.Size LedSize => new Avalonia.Size(Width, Height);
    private float _relativeX;

    /// <summary>
    /// Relative X to parrent
    /// </summary>
    public float RelativeX
    {
        get => _relativeX;
        set => SetProperty(ref _relativeX, value);
    }

    private float _relativeY;

    /// <summary>
    /// Relative Y to parent
    /// </summary>
    public float RelativeY
    {
        get => _relativeY;
        set => SetProperty(ref _relativeY, value);
    }

    /// <summary>
    /// offset X (parent's X)
    /// </summary>
    public float OffsetX => Device != null ? Device.X : 0f;

    /// <summary>
    /// Offset Y ( parent's Y)
    /// </summary>
    public float OffsetY => Device != null ? Device.Y : 0f;


    /// <summary>
    /// Absolute X position
    /// </summary>
    public float AbsoluteX => OffsetX + RelativeX;

    /// <summary>
    /// Absolute Y position
    /// </summary>
    public float AbsoluteY => OffsetY + RelativeY;

    /// <summary>
    /// Rect respect to rotation and scale on great bitmap
    /// </summary>
    public Rect TransformedRect { get; set; }

    public string Name { get; set; }

    #region Iposition aware implement
    //since 6.0.8, ambinity device will default be locked to prevent accidental modification
    private bool _isDragable = false;
    private bool _isSelectable = true;
    private bool _isSelected;
    private bool _isDeleteable = true;
    private bool _isResizeable = true;
    private bool _isRotatable;
    private bool _isScalable;

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
    /// Device can or can not be selected on the canvas
    /// </summary>
    [JsonIgnore]
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
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
        get => _isDragable;
        set => SetProperty(ref _isDragable, value);
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
            //DeviceUpdate?.Invoke();
        }
    }

    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
            //DeviceUpdate?.Invoke();
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

    /// <summary>
    /// set scale ot match canvas size
    /// </summary>
    private float _scale = 1.0f;

    private float _rotation = 0;

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            OnPropertyChanged();
        }
    }

    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            OnPropertyChanged();
        }
    }
    #endregion
    public string Icon => null;

    public Rect Bound => new Rect(X, Y, Width, Height);

    public Guid GroupID { get; set; } = Guid.Empty;

    public ContainerFigure GetContainer()
    {
        return new LEDContainerFigure(X, Y, Width, Height)
        {
            IsResizable = true,
            IsSelectable = true,
            IsDragable = true,
        };
    }

    public ContainerFigure Clone(float x, float y)
    {
        var cloneLed = ObjectHelpers.Clone<AmbinityLED>(this);
        var movX = x - X;
        var movY = y - Y;
        cloneLed.X = x;
        cloneLed.Y = y;
        // if (Shape == ZoneShapeEnum.Polyline)
        // {
        //     var newPoints = new List<Point>();
        //     foreach (var point in cloneZone.Points)
        //     {
        //         var newPoint = new Point(point.X + movX, point.Y + movY);
        //         newPoints.Add(newPoint);
        //     }

        //     cloneZone.Points = newPoints;
        // }

        var cloneContainerFigure = cloneLed.GetContainer();
        cloneContainerFigure.SetChild(cloneLed);
        return cloneContainerFigure;
    }

    public ContainerFigure Clone()
    {
        var cloneLed = ObjectHelpers.Clone<AmbinityLED>(this);
        var cloneContainerFigure = cloneLed.GetContainer();
        cloneContainerFigure.SetChild(cloneLed);
        return cloneContainerFigure;
    }

    public string GetDisplayName()
    {
        return Name;
    }

    public void SetScale(float scale)
    {
        throw new NotImplementedException();
    }

    public Avalonia.Media.Color? GetDisplayColor()
    {
        throw new NotImplementedException();
    }

    public void SetRotation(float angle)
    {
        throw new NotImplementedException();
    }

    public void SetX(float x)
    {
        throw new NotImplementedException();
    }

    public void SetY(float y)
    {
        throw new NotImplementedException();
    }

    public void SetWidth(float width)
    {
        throw new NotImplementedException();
    }

    public void SetHeight(float height)
    {
        throw new NotImplementedException();
    }


    #region Methods

    #endregion
}
