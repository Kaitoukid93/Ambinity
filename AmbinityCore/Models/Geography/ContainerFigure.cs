using AmbinityCore.Visualizer;
using Avalonia.Controls.Shapes;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;
using Draw2D.Core;
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
        if (ItemVisualizer == null)
            return;
        ItemVisualizer.UpdateContainerSize(newWidth, newHeight);
    }

    private void OnPositionChanged(float dx, float dy)
    {
        //update zone
        if (ItemVisualizer == null)
            return;
        ItemVisualizer.UpdateContainerOffset(dx, dy);
    }
     public override Figure Select(bool showHandles = true, bool repaint = true, bool notify = true)
    {
        IsSelectionActive = true;
        base.Select(showHandles, repaint, notify);

        // Add all other DeviceContainerFigure with the same ChildItem.GroupID to canvas selection, without notify
        if (ChildItem != null && ChildItem.GroupID != null && Canvas != null)
        {
            var groupId = ChildItem.GroupID;
            if (groupId == Guid.Empty)
                return this;
            var figures = Canvas.Figures?.OfType<ContainerFigure>()
                .Where(f => f != this && f.ChildItem != null && f.ChildItem.GroupID == groupId)
                .ToList();
            if (figures != null)
            {
                foreach (var fig in figures)
                {
                    if (!Canvas.Selection.Contains(fig))
                    {
                        Canvas.Selection.Add(fig, false); // false = do not notify
                    }
                }
            }
        }
        return this;
    }

    public override Figure Unselect()
    {
        IsSelectionActive = false;
        base.Unselect();
        return this;
    }

}
