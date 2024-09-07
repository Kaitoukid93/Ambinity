using AmbinityCore.Models.Geography;

namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneGroupContainerFigure : ContainerFigure
{
    public LightingZoneGroupContainerFigure(float x, float y, float width, float height) : base(x, y, width, height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}