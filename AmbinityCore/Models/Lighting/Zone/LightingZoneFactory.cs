namespace AmbinityCore.Models.Lighting.Zone;

public class LightingZoneFactory
{
    public LightingZoneFactory()
    {
        
    }
/// <summary>
/// Return a zone figure for a zone
/// </summary>
/// <param name="zone"></param>
/// <returns></returns>
    public LightingZoneFigure GetZoneFigure(LightingZone zone)
    {
        var zoneFigure = new LightingZoneFigure(zone.X, zone.Y, zone.Width, zone.Height);
        return zoneFigure;
    }
}