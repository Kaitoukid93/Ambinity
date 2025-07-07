using AmbinityCore.Models.Geography;
using AmbinityCore.Models.Lighting.Zone;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Draw2D.Core;

namespace AmbinityCore.Visualizer;

public interface ICanvasVisualizerItem
{
    event Action RefreshVisualizer;
    /// <summary>
    /// Raise when actual item get updated
    /// </summary>
    event Action ItemUpdated;
    /// <summary>
    /// Bound of item
    /// </summary>
    Rect Bounds { get; }
    /// <summary>
    /// Actual item to be visualized
    /// </summary>
    IPositionAware Item { get; }
    /// <summary>
    /// Update position of container on canvas
    /// </summary>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    void UpdateContainerOffset(float dx, float dy);
    /// <summary>
    /// Update size of container
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void UpdateContainerSize(float x, float y);
    /// <summary>
    /// render visualizer
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="canvas"></param>
    void Render(DrawingContext dc, Canvas canvas, bool isSelected = false);
}
