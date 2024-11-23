using AmbinityCore.Models.Collection;
using AmbinityServer.OnlineItem;
using Avalonia;
using Avalonia.Media;

namespace AmbinityCore.Models.Geography;

/// <summary>
/// Interface for any object that can be drop on Draw2D canvans
/// </summary>
public interface IPositionAware
{
    string Name { get; }

    /// <summary>
    /// X value of this object respect to the canvas coordinate system
    /// </summary>
    float X { get; set; }

    /// <summary>
    /// Y value of this object respect to the canvas coordinate system
    /// </summary>
    float Y { get; set; }

    /// <summary>
    /// Width value of this object respect to the canvas coordinate system
    /// </summary>
    float Width { get; set; }

    /// <summary>
    /// Height value of this object respect to the canvas coordinate system
    /// </summary>
    float Height { get; set; }

    /// <summary>
    /// Scale value of this object
    /// </summary>
    float Scale { get; set; }

    /// <summary>
    /// Rotation value of this object
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// Indicate scalable property of this object
    /// </summary>
    bool IsScalable { get; set; }

    /// <summary>
    /// Indicate rotateable property of this object
    /// </summary>
    bool IsRotatable { get; set; }

    /// <summary>
    /// Indicate selectable property of this object
    /// </summary>
    bool IsSelectable { get; set; }
    /// <summary>
    /// Indicate Selected property of this object
    /// </summary>
    bool IsSelected { get; set; }
    /// <summary>
    /// Indicate resizeable property of this object
    /// </summary>
    bool IsResizeable { get; set; }

    /// <summary>
    /// Indicate Draggable property of this object
    /// </summary>
    bool IsDraggable { get; set; }

    /// <summary>
    /// Indicate Deleteable property of this object
    /// </summary>
    bool IsDeleteable { get; set; }

    ContainerFigure GetContainer();
    ContainerFigure Clone(float x, float y);
    string GetDisplayName();
    string Icon { get; }
    Rect Bound { get; }
    Guid GroupID { get; set; }
    void SetScale(float scale);
    Color? GetDisplayColor();
    void SetRotation(float angle);
    void SetX(float x);
    void SetY(float y);
    void SetWidth(float width);
    void SetHeight(float height);

}