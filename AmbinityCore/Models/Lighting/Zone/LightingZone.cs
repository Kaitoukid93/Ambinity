using AmbinityCore.Helpers;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using AmbinityCore.Models.Profile;
using AmbinityCore.Repositories;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Draw2D.Core.Graphic;
using Newtonsoft.Json;

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
    private Color? _displayColor = null;

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

    public Guid GroupID { get; set; }

    public void SetScale(float scale)
    {
        //throw new NotImplementedException();
    }

    public void SetRotation(float angle)
    {
        // throw new NotImplementedException();
    }

    public void SetX(float x)
    {
        if (X != x)
            X = x;
    }

    public void SetY(float y)
    {
        if (Y != y)
            Y = y;
    }

    public void SetWidth(float width)
    {
        if (Width != width)
            Width = width;
    }

    public void SetHeight(float height)
    {
        if (Height != height)
            Height = height;
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
    private bool _isRotatable = false;
    private bool _isDraggable = true;
    private bool _isScalable = false;

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
        if (Shape == ZoneShapeEnum.Polyline)
            this.IsResizeable = false;
        return new LightingZoneFigure(X, Y, Width, Height)
        {
            IsResizable = this.IsResizeable,
            IsSelectable = this.IsSelectable,
            IsDragable = this.IsDraggable,
            MinWidth = 2,
            MinHeight = 2,
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
        var movX = x - X;
        var movY = y - Y;
        cloneZone.X = x;
        cloneZone.Y = y;
        if (Shape == ZoneShapeEnum.Polyline)
        {
            var newPoints = new List<Point>();
            foreach (var point in cloneZone.Points)
            {
                var newPoint = new Point(point.X + movX, point.Y + movY);
                newPoints.Add(newPoint);
            }

            cloneZone.Points = newPoints;
        }

        var cloneContainerFigure = cloneZone.GetContainer();
        cloneContainerFigure.SetChild(cloneZone);
        return cloneContainerFigure;
    }

    public Rect Bound => ZoneBound;

    [JsonIgnore] public CollectableItemRepository LocalRepository { get; set; }

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
    public ZoneShapeEnum Shape { get; set; } // polyline, rectangle, ellipse
    public List<Point> Points { get; set; }

    /// <summary>
    /// get list of points this zone defined by
    /// </summary>
    public List<Point> GetPoints()
    {
        var points = new List<Point>();
        switch (Shape)
        {
            case ZoneShapeEnum.Rectangle:
                points = GeometryHelper.RectToPolygon(ZoneBound).ToList();
                break;
            case ZoneShapeEnum.Ellipse:
                points = GeometryHelper.ConvertEllipseToPolygon(Center, Width / 2, Height / 2, 5);
                break;
            case ZoneShapeEnum.Polyline:
                points = Points;
                break;
        }

        return points;
    }

    [JsonIgnore] public Rect ZoneBound => new Rect(X, Y, Width, Height);
    [JsonIgnore] public Point Center => new Point(X + Width / 2, Y + Height / 2);
    public string Name { get; set; }
    [JsonIgnore] public bool IsSelected { get; set; }
    [JsonIgnore] public bool IsEditing { get; set; }
    [JsonIgnore] public bool IsChecked { get; set; }
    [JsonIgnore] public bool IsPinned { get; set; }
    [JsonIgnore] public string Icon => GetIcon();

    private string GetIcon()
    {
        switch (Shape)
        {
            case ZoneShapeEnum.Ellipse:
                return "CanvasTool_Ellipse";
            case ZoneShapeEnum.Rectangle:
                return "CanvasTool_Rectangle";
            case ZoneShapeEnum.Polyline:
                return "CanvasTool_PolyLine";
            default: return null;
        }
    }

    public string GetDisplayName()
    {
        return Shape.ToString() + " - " + LightingConfiguration.Name;
    }

    public Color? GetDisplayColor()
    {
        if (_displayColor != null)
            return _displayColor;
        switch (LightingConfiguration.Type)
        {
            case ConfigurationType.ScreenCapture:
                _displayColor = Color.Parse("#d769ff");
                break;
            case ConfigurationType.Animation:
                _displayColor = Color.Parse("#ffb033");
                break;
            case ConfigurationType.Gifxelation:
                _displayColor = Avalonia.Media.Colors.White;
                break;
            case ConfigurationType.SelfGeneratedColor:
                _displayColor = Color.Parse("#33bbff");

                break;
        }

        return _displayColor;
    }

    [JsonIgnore] public LightingProfile ParentProfile { get; set; }
    [JsonIgnore] public string LocalPath { get; set; }

    public OnlineItemTypeEnum GetType()
    {
        return OnlineItemTypeEnum.LightingZone;
    }

    public void Save()
    {
        //todo implement profile save with icon 
        if (LocalPath == null || !Directory.Exists(LocalPath))
        {
            //create local path
            var dbPath = LocalRepository.LocalFolderPath;
            LocalPath = Path.Combine(dbPath, Name);
            Directory.CreateDirectory(LocalPath);
        }

        JsonHelpers.WriteSimpleJson(this, Path.Combine(LocalPath, "config.json"));
    }
}