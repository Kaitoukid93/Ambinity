using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Graphic;
using AmbinityCore.Models.Lighting.Zone.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmbinityCore.Models.Lighting.Zone;

/// <summary>
/// Represent a zone that render on the main canvas 
/// </summary>
public class LightingZone : ObservableObject, ICollectableItem
{
    public event Action FrameBufferSizeUpdated;
    public event Action SizeUpdated;
    public event Action LocationUpdated;
    public LightingZone(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public LightingZone()
    {
    }

    #region Canvas Corordinate Properties

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
        set => SetProperty(ref _x, value);
    }

    /// <summary>
    /// Y property of this zone
    /// </summary>
    public float Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    private float _width = 100;
    private float _height = 100;

    /// <summary>
    /// Width property of this zone
    /// </summary>
    public float Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    /// <summary>
    /// Height property of this zone
    /// </summary>
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
    public FrameBuffer Buffer { get; set; }

    public void UpdateFrameBuffer()
    {
        Buffer.FrameWidth = (int)Width;
        Buffer.FrameHeight = (int)Height;
        
    }
    #endregion

    public event Action<ICollectableItem>? ItemNameChanged;
    public event Action<ICollectableItem>? ItemPinStatusChanged;
    public event Action<ICollectableItem>? ItemCheckStatusChanged;
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public bool IsEditing { get; set; }
    public bool IsChecked { get; set; }
    public bool IsPinned { get; set; }
    public string LocalPath { get; set; }
}