using System.Text.Json.Serialization;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Device.Device;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityServer.OnlineItem;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core.Graphic;

namespace AmbinityCore.Models.Lighting.Zone;

/// <summary>
/// Represent a zone that render on the main canvas 
/// </summary>
public class LightingZone : ObservableObject, ICollectableItem, IPositionAware
{
    public event Action FrameUpdated;
    public event Action SizeUpdated;
    public event Action LocationUpdated;
    public event Action<Rect> UserInputUpdateValidate;

    public LightingZone(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Buffer = new FrameBuffer((int)Width, (int)Height);
    }

    public LightingZone()
    {
    }

    #region Canvas Corordinate Properties

    private bool _isRendering;

    [JsonIgnore]
    public bool IsRendering
    {
        get => _isRendering;
        set
        {
            _isRendering = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// zone absolute position on the canvas respect ot top left corner
    /// </summary>
    private float _x = 0;

    private float _y = 0;

    /// <summary>
    /// X property of this zone
    /// </summary>
    public float X
    {
        get => _x;
        set
        {
            _x = value;
            OnPropertyChanged();
            LocationUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Y property of this zone
    /// </summary>
    public float Y
    {
        get => _y;
        set
        {
            _y = value;
            OnPropertyChanged();
            LocationUpdated?.Invoke();
        }
    }

    private float _width = 100;
    private float _height = 100;

    /// <summary>
    /// Width property of this zone
    /// </summary>
    public float Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
            SizeUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Height property of this zone
    /// </summary>
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
            SizeUpdated?.Invoke();
        }
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

    public void TryUpdate(Rect rect)
    {
        UserInputUpdateValidate?.Invoke(rect);
    }

    #endregion

    #region Iposition aware implement

    private bool _isDragable = true;
    private bool _isSelectable = true;
    private bool _isDeleteable = true;
    private bool _isResizeable = true;
    private bool _isHitTestVisible = true;
    private bool _isRotatable;
    private bool _isDraggable;
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
        return new LightingZoneFigure(X, Y, Width, Height)
        {
            IsResizable = this.IsResizeable,
            IsSelectable = this.IsSelectable,
            IsDragable = this.IsDraggable,
        };
    }

    /// <summary>
    /// clone this object and put it at certain point on canvas
    /// </summary>
    /// <param name="x"></param>
    /// <param name="Y"></param>
    /// <returns></returns>
    public ContainerFigure Clone(float x, float y)
    {
        var cloneZone = ObjectHelpers.Clone<LightingZone>(this);
        cloneZone.X = x;
        cloneZone.Y = y;
        var cloneContainerFigure = new LightingZoneFigure(x, y,Width, Height);
        cloneContainerFigure.SetChild(cloneZone);
        return cloneContainerFigure;
    }

    public CollectableItemRepository GetLocalRepository()
    {
        return Ioc.Default.GetRequiredService<LightingZoneRepository>();
    }

    public OnlineItemRepository GetOnlineRerpository()
    {
        return Ioc.Default.GetRequiredService<LightingZoneOnlineRepository>();
    }

    #endregion

    #region Lighting

    public ILightingConfiguration LightingConfiguration { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Graphic

    /// <summary>
    /// Frame data 
    /// </summary>
    [JsonIgnore]
    public FrameBuffer Buffer { get; set; }

    public void UpdateFrameBuffer()
    {
        if (Buffer == null)
        {
            Buffer = new FrameBuffer((int)Width, (int)Height);
        }

        Buffer.FrameWidth = (int)Width;
        Buffer.FrameHeight = (int)Height;
        Buffer.UpdatePixelData();
    }

    public void UpdateFrame()
    {
        FrameUpdated?.Invoke();
    }

    #endregion


    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }
    public string LocalPath { get; set; }

    public void Save()
    {
        JsonHelpers.WriteSimpleJson(this, LocalPath);
    }
}