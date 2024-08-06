using AmbinityCore.Visualizer;
using Avalonia.Controls.Shapes;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;

namespace AmbinityCore.Models.Geography;

/// <summary>
/// Base class for container of canvas item
/// </summary>
public abstract class ContainerFigure : Rectangle
{
    public ContainerFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        PositionPropertyChanged += OnPositionChanged;
        SizePropertyChanged += OnSizeChanged;
    }
    public IPositionAware ChildItem { get; set; }
    public ICanvasVisualizerItem ItemVisualizer { get; set; }
    public bool IsValid { get; set; }
    public virtual void SetChild(IPositionAware child)
    {
        
    }
    private void OnSizeChanged(float newWidth, float newHeight)
    {
        //update zone
        ItemVisualizer.UpdateContainerSize(newWidth, newHeight);
    }

    private void OnPositionChanged(float dx, float dy)
    {
        //update zone
        ItemVisualizer.UpdateContainerOffset(dx, dy);
    }
    
}