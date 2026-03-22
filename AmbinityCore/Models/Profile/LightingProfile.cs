using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.Profile;
using Avalonia.Media;

public class LightingProfile
{
    public Guid ID { get; set; }
    public Guid CategoryID { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public IconTypeEnum IconType { get; set; }
    public string Icon { get; set; }
    public Color IconColor { get; set; }

    public int Brightness { get; set; } = 100;

    public List<LightingZone> Zones { get; set; } = new();
}
