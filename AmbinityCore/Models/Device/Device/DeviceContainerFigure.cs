using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityCore.Repositories;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Draw2D.Core.Policies.FigurePolicy;
using Draw2D.Core.Shapes.Basic;
using Draw2D.Core.Utlils;
using Draw2D.Core;

namespace AmbinityCore.Models.Device;

public class DeviceContainerFigure : ContainerFigure, IAssetSelectable
{
    public DeviceContainerFigure(float x, float y, float width, float height) : base(x, y, width,
        height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        // SnapTargets = SnapTargets.Center | SnapTargets.MidPoints | SnapTargets.Vertices;
    }

    public override void SetChild(IPositionAware child)
    {
        ChildItem = child;
        ItemVisualizer = new DeviceVisualizer(child);
        ItemVisualizer.ItemUpdated += OnItemUpdate;
        ItemVisualizer.RefreshVisualizer += OnItemVisualizerUpdated;
        Width = (float)ItemVisualizer.Bounds.Width;
        Height = (float)ItemVisualizer.Bounds.Height;
        X = (float)ItemVisualizer.Bounds.X;
        Y = (float)ItemVisualizer.Bounds.Y;
    }
    private void OnItemUpdate()
    {
        Width = (float)ItemVisualizer.Bounds.Width;
        Height = (float)ItemVisualizer.Bounds.Height;
        X = (float)ItemVisualizer.Bounds.X;
        Y = (float)ItemVisualizer.Bounds.Y;
        Canvas.NeedsRepaint(this);
    }

    private void OnItemVisualizerUpdated()
    {
        Canvas.NeedsRepaint(this);
    }
    public override void Render(DrawingContext dc, double strokeThickness, Color strokeColor)
    {
        //get size and location from device property
        ItemVisualizer.Render(dc, Canvas);
        var strokeBrush = new ImmutableSolidColorBrush(StrokeColor);
        var thickness = StrokeThickness;
        if (OverrideStrokeStyle)
        {
            strokeBrush = new ImmutableSolidColorBrush(strokeColor);
            thickness = (float)strokeThickness;
        }
        var _canvasRect = new Rect(0, 0, Canvas.Width, Canvas.Height);
        var rect = new Rect(X, Y, Width, Height);
        bool _isValid = _canvasRect.Contains(rect);
        var screenPoint = Canvas.CoordinateSystem.ToScreenSpace(Position);
        var offset = new Point((float)screenPoint[0] - X, (float)screenPoint[1] - Y);
        var fillBrush = new ImmutableSolidColorBrush(FillColor);
        if (!_isValid)
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Red);
        if (IsMouseOver && !IsSelected)
        {
            fillBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Gray.AdjustOpacity(0.2));
            strokeBrush = new ImmutableSolidColorBrush(Avalonia.Media.Colors.Orange);
        }

        var pen = new Pen(strokeBrush, thickness, DashStyle);
        var immutablePen = pen.ToImmutable();
        Matrix translate = Matrix.CreateTranslation(offset.X, offset.Y);
        dc.PushTransform(translate);
        if (IsSelectable)
            dc.DrawRectangle(fillBrush, immutablePen,
                new Rect(new Point(X, Y), new Size(Width, Height)));

        // dc.Pop();
    }

    #region IAssetSelectable implementation


    public ICollectableItem AssetSelectableProperty => (ChildItem as AmbinityDevice).Layout;

    #endregion

    public bool IsSelectionActive { get; private set; } = false;

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
            var figures = Canvas.Figures?.OfType<DeviceContainerFigure>()
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
