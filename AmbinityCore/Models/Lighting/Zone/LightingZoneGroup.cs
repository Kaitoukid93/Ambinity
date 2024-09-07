using System.Drawing;
using System.Text.Json.Serialization;
using AmbinityCore.Helpers;
using AmbinityCore.Models.Collection;
using AmbinityCore.Models.Geography;
using AmbinityServer.OnlineItem;
using CommunityToolkit.Mvvm.ComponentModel;
using Rectangle = Draw2D.Core.Shapes.Basic.Rectangle;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneGroup : Group
{
    public LightingZoneGroup()
    {
        GroupID = Guid.NewGuid();
    }

    public override ContainerFigure GetContainer()
    {
        return new LightingZoneGroupContainerFigure(X, Y, Width, Height)
        {
            IsResizable = this.IsResizeable,
            IsSelectable = this.IsSelectable,
            IsDragable = this.IsDraggable,
        };
    }

    public override ContainerFigure Clone(float x, float y)
    {
        throw new NotImplementedException();
    }
    public Guid GroupID { get; set; }

    public void AddChild(IPositionAware child)
    {
        child.GroupID = this.GroupID;
        Child.Add(child);
    }
}