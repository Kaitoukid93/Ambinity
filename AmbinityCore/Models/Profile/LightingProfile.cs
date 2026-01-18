using System.Collections.ObjectModel;
using AmbinityCore.Models.Lighting.Zone;
using AmbinityCore.Models.ProfileCategory;
using Avalonia.Media;
using Newtonsoft.Json;

namespace AmbinityCore.Models.Profile;

/// <summary>
/// Pure domain model for a lighting profile.
///
/// </summary>
public sealed class LightingProfile
{
    // =========================
    // Identity
    // =========================
    public Guid ID { get; set; }

    // =========================
    // Metadata
    // =========================
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public bool IsDefault { get; set; }

    // =========================
    // Display metadata (still domain)
    // =========================
    public IconTypeEnum IconType { get; set; }
    public Color IconColor { get; set; }

    // =========================
    // Lighting behavior
    // =========================
    public int Brightness { get; set; } = 100;

    // =========================
    // Zones
    // =========================
    public ObservableCollection<LightingZone> Zones { get; } = new();

    [JsonIgnore]
    public int ZoneCount => Zones.Count;

    // =========================
    // Domain behavior
    // =========================
    public void AddZone(LightingZone zone)
    {
        if (zone == null)
            throw new ArgumentNullException(nameof(zone));

        zone.ParentProfile = this;
        Zones.Add(zone);
    }

    public void RemoveZone(LightingZone zone)
    {
        if (zone == null)
            return;

        Zones.Remove(zone);
    }
}
