using System.Drawing;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Draw2D.Core;
using SkiaSharp;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;

namespace AmbinityCore.Models.Device.LED;

public class AmbinityLED : ObservableObject
{
    /// <summary>
    /// extend ARGBLED with size and location on a real device
    /// </summary>
    public AmbinityLED(ArgbLed led, AmbinityDevice device,
        float left,
        float top,
        float width,
        float height,
        int index,
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
    }

    public ArgbLed LED { get; }
    public int Index { get; set; }
    [JsonIgnore]
    public AmbinityDevice Device { get; }

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

    private int _ledID = 0;

    /// <summary>
    /// LED ID map to real world
    /// </summary>
    public int LedID
    {
        get => _ledID;
        set => SetProperty(ref _ledID, value);
    }

    private float _width;

    /// <summary>
    /// Bounding box width
    /// </summary>
    public float Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private float _height;

    /// <summary>
    /// Bounding box width
    /// </summary>
    public float Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
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
    public float OffsetX => Device.X;

    /// <summary>
    /// Offset Y ( parent's Y)
    /// </summary>
    public float OffsetY => Device.Y;


    /// <summary>
    /// Absolute X position 
    /// </summary>
    public float AbsoluteX => OffsetX + RelativeX;

    /// <summary>
    /// Absolute Y position
    /// </summary>
    public float AbsoluteY => OffsetY + RelativeY;

    #region Methods

    #endregion
}